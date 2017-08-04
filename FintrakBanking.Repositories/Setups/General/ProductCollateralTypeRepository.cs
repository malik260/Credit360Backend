using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.Repositories.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Setups.Finance
{
    /// <summary>
    /// TODO: Implement audit trails in these methods
    /// </summary>
      [Export(typeof(IProductCollateralTypeRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ProductCollateralTypeRepository : IProductCollateralTypeRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository _genSetup;
        public ProductCollateralTypeRepository(FinTrakBankingContext _context, 
                                                IAuditTrailRepository _auditTrail,
                                                IGeneralSetupRepository genSetup)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public int AddProductCollateralType(ProductCollateralTypeViewModel productCollateral)
        {
            var dataExist = this.context.tbl_Product_CollateralType.FirstOrDefault(x => x.ProductId == productCollateral.productId && x.CollateralTypeId == productCollateral.collateralTypeId && x.Deleted == true); // .Find(accountId);

            var productCollateralEntity = dataExist;

            if (dataExist == null)
            {
                productCollateralEntity = new tbl_Product_CollateralType()
                {
                    ProductId = productCollateral.productId,
                    CollateralTypeId = productCollateral.collateralTypeId,
                    CompanyId = productCollateral.companyId,
                    CreatedBy = productCollateral.createdBy,
                    DateTimeCreated = _genSetup.GetApplicaionDate()
                };

                this.context.tbl_Product_CollateralType.Add(productCollateralEntity);
            }
            else
            {
                productCollateralEntity.Deleted = false;
            }
            var status = this.SaveAll();

            // Audit Section ---------------------------
            var product = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == productCollateral.productId);
            var collateralInfo = this.context.tbl_Collateral_Type.FirstOrDefault(x => x.CollateralTypeId == productCollateral.collateralTypeId);
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductCollateralAdded,
                StaffId = productCollateral.createdBy,
                BranchId = (short)productCollateral.userBranchId,
                Detail = "Added product collateral type: " + collateralInfo.CollateralTypeName + " to product " + product.ProductCode + " (" + product.ProductName + ")",
                IPAddress = productCollateral.userIPAddress,
                Url = productCollateral.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = _genSetup.GetApplicaionDate()


            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            if (status)
                return productCollateralEntity.ProductCollateralTypeId;
            else
                return -1;
        }

        public int AddTempProductCollateralType(ProductCollateralTypeViewModel productCollateral)
        {
            var dataExist = this.context.tbl_Temp_Product_CollateralType.FirstOrDefault(x => x.ProductId == productCollateral.productId
                                                                && x.CollateralTypeId == productCollateral.collateralTypeId
                                                                && x.Deleted == true); // .Find(accountId);

            var tempProductCollateralEntity = dataExist;

            if (dataExist == null)
            {
                tempProductCollateralEntity = new tbl_Temp_Product_CollateralType()
                {
                    ProductId = productCollateral.productId,
                    CompanyId = productCollateral.companyId,
                    CollateralTypeId = productCollateral.collateralTypeId,
                    CreatedBy = productCollateral.createdBy,
                    DateTimeCreated = _genSetup.GetApplicaionDate(),
                    Deleted = false,
                    IsCurrent = true
                };

                var existingProductApprovalLog = context.tbl_Temp_Product.Find(productCollateral.productId);
                var ProductData = context.tbl_Temp_Product.Find(productCollateral.productId);

                if (existingProductApprovalLog != null)
                {
                    ProductData.IsCurrent = true;
                }

                this.context.tbl_Temp_Product_CollateralType.Add(tempProductCollateralEntity);
                // Audit Section ---------------------------
                var product = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == productCollateral.productId);
                var collateralInfo = this.context.tbl_Collateral_Type.FirstOrDefault(x => x.CollateralTypeId == productCollateral.collateralTypeId);
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.ProductCollateralAdded,
                    StaffId = productCollateral.createdBy,
                    BranchId = (short)productCollateral.userBranchId,
                    Detail = "Added product collateral type: " + collateralInfo.CollateralTypeName + " to product " + product.ProductCode + " (" + product.ProductName + ")",
                    IPAddress = productCollateral.userIPAddress,
                    Url = productCollateral.applicationUrl,
                    SystemDateTime = DateTime.Now,
                    ApplicationDate = _genSetup.GetApplicaionDate()
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------
            }
            else
            {
                tempProductCollateralEntity.ProductId = productCollateral.productId;
                tempProductCollateralEntity.CollateralTypeId = productCollateral.collateralTypeId;

                tempProductCollateralEntity.Deleted = false;
            }

            var status = this.SaveAll();

            if (status)
                return tempProductCollateralEntity.ProductCollateralTypeId;
            else
                return -1;
        }

        public void ApproveProductCollateral(int productId, UserInfo user)
        {
            var productCollateralTypeModel = context.tbl_Temp_Product_CollateralType.Where(x => x.ProductId == productId
                                                                        && x.Deleted == false
                                                                        && x.IsCurrent == true);
            var productToUpdate = context.tbl_Product.Find(productId);

            foreach (var p in productCollateralTypeModel)
            {
                var productCollateralType = new tbl_Product_CollateralType()
                {
                    ProductId = p.ProductId,
                    CompanyId = p.CompanyId,
                    CreatedBy = p.CreatedBy,
                    DateTimeCreated = _genSetup.GetApplicaionDate(),
                    Deleted = false
                };
                context.tbl_Product_CollateralType.Add(productCollateralType);
                context.tbl_Temp_Product_CollateralType.Remove(p);
            }

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductFeeAdded,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Added CollateralType for product '{productToUpdate.ProductName}' with product code'{productToUpdate.ProductCode}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            //return this.SaveAll();
        }

        public int AddMultipleProductCollateralType(List<ProductCollateralTypeViewModel> collateralTypes)
        {
            if (collateralTypes.Count <= 0)
                return -1;

            foreach (ProductCollateralTypeViewModel item in collateralTypes)
            {
                AddProductCollateralType(item );
            }

            return 1;
        }

        public bool DoesProductCollateralExist(int productCollateralTypeId)
        {
            return context.tbl_Product_CollateralType.Any(x => x.ProductCollateralTypeId == productCollateralTypeId);
        }

        public IEnumerable<ProductCollateralTypeViewModel> GetCollateralTypeByProduct(int productId)
        {
            return (from data in context.tbl_Product_CollateralType
                    where data.ProductId == productId && data.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new ProductCollateralTypeViewModel()
                    {
                        productCollateralId = data.ProductCollateralTypeId,
                        productId = (short)data.ProductId,
                        collateralTypeId = data.CollateralTypeId,
                        collateralTypeName = data.tbl_Collateral_Type.CollateralTypeName,
                        companyId = data.CompanyId,

                        createdBy = data.CreatedBy,
                        //dateTimeCreated = data.DateTimeCreated,
                        //lastUpdatedBy = data.LastUpdatedBy,
                        //dateTimeUpdated = data.DateTimeUpdated,

                        //deleted = data.Deleted,
                        //deletedBy = data.DeletedBy,
                        //dateTimeDeleted = data.DateTimeDeleted
                        //lastUpdatedBy = data.LastUpdatedBy.Value,
                        //dateTimeUpdated = data.DateTimeUpdated,

                        deleted = data.Deleted,
                       // deletedBy = data.DeletedBy,
                       // dateTimeDeleted = data.DateTimeDeleted

                    });
        }

        public IEnumerable<CollateralTypeViewModel> GetUnmappedCollateralToProduct(int productId)
        {
            IEnumerable<CollateralTypeViewModel> collaterals = null;

            var dataList = (from data in context.tbl_Product_CollateralType
                            where data.ProductId == productId && data.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                            select data.CollateralTypeId).ToList();

            collaterals = (from a in context.tbl_Collateral_Type
                           where a.Deleted == false
                           select new CollateralTypeViewModel
                           {
                              // collateralCategoryName = a.CollateralCategory.CollateralCategoryName,
                              // collateralCategoryId = a.CollateralCategoryId,
                               collateralTypeName = a.CollateralTypeName,
                               collateralTypeId = a.CollateralTypeId,
                               companyId = a.CompanyId,
                               //dateTimeUpdated = a.DateTimeUpdated,
                               deleted = a.Deleted,
                               details = a.Details,
                               //hairCut = a.HairCut,
                              // requiresLocation = a.RequiresLocation,
                               //lastUpdatedBy = a.LastUpdatedBy ?? 0,
                               createdBy = a.CreatedBy,
                               dateTimeCreated = a.DateTimeCreated 
                           });

            if (dataList.Any())
            {
                collaterals = collaterals.Where(x => !dataList.Contains(x.collateralTypeId));
            }

            return collaterals;
        }

        public ProductCollateralTypeViewModel GetProductCollateralTypeViewModel(int productCollateralTypeId)
        {
            return (from data in context.tbl_Product_CollateralType
                    where data.ProductCollateralTypeId == productCollateralTypeId && data.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new ProductCollateralTypeViewModel()
                    {
                        productCollateralId = data.ProductCollateralTypeId,
                        productId =(short)data.ProductId,
                        collateralTypeId = data.CollateralTypeId,
                        collateralTypeName = data.tbl_Collateral_Type.CollateralTypeName,
                        companyId = data.CompanyId,

                        createdBy = data.CreatedBy,
                        //dateTimeCreated = data.DateTimeCreated,

                        //lastUpdatedBy = data.LastUpdatedBy.Value,
                       // dateTimeUpdated = data.DateTimeUpdated,

                        deleted = data.Deleted,
                       // deletedBy = data.DeletedBy,
                       // dateTimeDeleted = data.DateTimeDeleted
                    }).FirstOrDefault();
        }

        public bool DeleteProductCollateralType(int productCollateralTypeId, UserInfo user)

        {
            var data = this.context.tbl_Product_CollateralType.Find(productCollateralTypeId);

            if (data == null)
                return false;

            data.Deleted = true;
            data.DateTimeDeleted = _genSetup.GetApplicaionDate();

            // Audit Section ---------------------------

            
            var collateralInfo = (from x in context.tbl_Product_CollateralType
                                  where x.ProductCollateralTypeId == productCollateralTypeId  //orderby account.AccountCode ascending, account.AccountName ascending
                                  select new
                                  {

                                      collateralTypeName = x.tbl_Collateral_Type.CollateralTypeName,
                                      productCode = x.tbl_Product.ProductCode,
                                      productName = x.tbl_Product.ProductName

                                  }).FirstOrDefault();

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductCollateralDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = "Deleted product collateral type: " + collateralInfo.collateralTypeName + " to product " + collateralInfo.productCode + " (" + collateralInfo.productName + ")",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //AuditTrail
            
            return this.SaveAll();

            
        }

        public bool DeleteMultipleProductCollateralType(List<int> productCollateralTypeIds, UserInfo user)
        {
            if (productCollateralTypeIds.Count <= 0)
                return false;

            var dataList = (from a in context.tbl_Product_CollateralType
                            where productCollateralTypeIds.ToList().Contains(a.ProductCollateralTypeId)
                            select a);

            foreach (tbl_Product_CollateralType data in dataList)
            {
                data.Deleted = true;
                data.DateTimeDeleted = DateTime.Now;   
                // Audit Section ---------------------------

            var collateralType = this.context.tbl_Product_CollateralType.FirstOrDefault(x => x.ProductCollateralTypeId == data.ProductCollateralTypeId);

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductCollateralDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = "Deleted product collateral type: " + collateralType.tbl_Collateral_Type.CollateralTypeName + " to product " + collateralType.tbl_Product.ProductCode + " (" + collateralType.tbl_Product.ProductName + ")",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            }
     

            return this.SaveAll();
        }
    }
}