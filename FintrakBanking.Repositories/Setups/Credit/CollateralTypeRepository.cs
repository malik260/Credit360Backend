using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Repositories;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Credit
{
    [Export(typeof(ICollateralTypeRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CollateralTypeRepository : ICollateralTypeRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;

        public CollateralTypeRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository genSetup, IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
        }

        //public async Task<bool> AddCollateralCategory(CollateralCategoryViewModel entity)
        //{
        //    var category = new TblCollateralCategory
        //    {
        //        CollateralCategoryName = entity.collateralCategoryName,
        //        CompanyId = entity.companyId,
        //        CreatedBy = (int)entity.createdBy,
        //        DateTimeCreated = DateTime.UtcNow,
        //        IsProduct = entity.isProduct,
        //        ProductGroupId = entity.productGroupId
        //    };
        //    context.TblCollateralCategory.Add(category);

        //    // Audit Section ---------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralCategoryAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Added Collateral Category: { entity.collateralCategoryName } ",
        //        IPAddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicaionDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------

        //    var respose = await context.SaveChangesAsync() != 0;
        //    return respose;
        //}

        //public async Task<bool> UpdateCollateralCategory(int categoryId, CollateralCategoryViewModel entity)
        //{
        //    var category = context.TblCollateralCategory.SingleOrDefault(c => c.CollateralCategoryId == categoryId);

        //    category.CompanyId = entity.companyId;
        //    category.DateTimeCreated = DateTime.UtcNow;
        //    category.IsProduct = entity.isProduct;
        //    category.DateTimeUpdated = entity.dateTimeUpdated;
        //    category.LastUpdatedBy = entity.lastUpdatedBy;
        //    category.ProductGroupId = entity.productGroupId;
        //    category.CollateralCategoryName = entity.collateralCategoryName;
        //    var respose = await context.SaveChangesAsync() != 0;

        //    // Audit Section ---------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralCategoryUpdated,
        //        StaffId = entity.createdBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Updated Collateral Category: { entity.collateralCategoryName } ",
        //        IPAddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicaionDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------

        //    return respose;
        //}

        //public async Task<bool> DeleteCollateralCategory(int categoryId, CollateralCategoryViewModel entity, UserInfo user)
        //{
        //    var category = context.TblCollateralCategory.SingleOrDefault(c => c.CollateralCategoryId == categoryId);

        //    category.DateTimeCreated = DateTime.UtcNow;
        //    category.DeletedBy = entity.deletedBy;
        //    category.Deleted = true;

        //    // Audit Section ---------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CollateralCategoryDeleted,
        //        StaffId = user.staffId,
        //        BranchId = (short)user.BranchId,
        //        Detail = $"Deleted Collateral Category: { entity.collateralCategoryName } ",
        //        IPAddress = user.userIPAddress,
        //        Url = user.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicaionDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------
        //    return await context.SaveChangesAsync() != 0;
        //}

        //public IEnumerable<CollateralCategoryViewModel> GetCollateralCategory()
        //{
        //    var category = (from a in context.TblCollateralCategory
        //                    where a.Deleted == false
        //                    select new CollateralCategoryViewModel
        //                    {
        //                        collateralCategoryId = a.CollateralCategoryId,
        //                        collateralCategoryName = a.CollateralCategoryName,
        //                        companyId = (short)a.CompanyId,
        //                        createdBy = a.CreatedBy,
        //                        dateTimeCreated = DateTime.UtcNow,
        //                        isProduct = a.IsProduct,
        //                        productGroupId = (short)a.ProductGroupId
        //                    }).ToList();
        //    return category;
        //}

        //public CollateralCategoryViewModel GetCollateralCategoryById(int categoryId)
        //{
        //    var category = (from a in context.TblCollateralCategory
        //                    where a.CollateralCategoryId == categoryId && a.Deleted == false
        //                    select new CollateralCategoryViewModel
        //                    {
        //                        collateralCategoryId = a.CollateralCategoryId,
        //                        collateralCategoryName = a.CollateralCategoryName,
        //                        companyId = a.CompanyId,
        //                        createdBy = a.CreatedBy,
        //                        dateTimeCreated = a.DateTimeCreated,
        //                        isProduct = a.IsProduct,
        //                        productGroupId = a.ProductGroupId
        //                    }).SingleOrDefault();
        //    return category;
        //}

        //public IEnumerable<CollateralCategoryViewModel> GetCollateralCategoryByProductGroupId(int ProductGroupId)
        //{
        //    var category = (from a in context.TblCollateralCategory
        //                    where a.ProductGroupId == ProductGroupId && a.Deleted == false
        //                    select new CollateralCategoryViewModel
        //                    {
        //                        collateralCategoryId = a.CollateralCategoryId,
        //                        collateralCategoryName = a.CollateralCategoryName,
        //                        companyId = a.CompanyId,
        //                        createdBy = a.CreatedBy,
        //                        dateTimeCreated = a.DateTimeCreated,
        //                        isProduct = a.IsProduct,
        //                        productGroupId = a.ProductGroupId
        //                    }).ToList();
        //    return category;
        //}

        public IEnumerable<CollateralTypeViewModel> GetCollateralTypes()
        {
            var category = (from a in context.tbl_Collateral_Type
                            where a.Deleted == false
                            select new CollateralTypeViewModel
                            {
                               // collateralCategoryName = context.TblCollateralCategory.SingleOrDefault(c => c.CollateralCategoryId == a.CollateralCategoryId).CollateralCategoryName,
                                //collateralCategoryId = a.CollateralCategoryId,
                                collateralTypeName = a.CollateralTypeName,
                                collateralTypeId = a.CollateralTypeId,
                                dateTimeUpdated = a.DateTimeUpdated,
                                deleted = a.Deleted,
                                details = a.Details,
                               // hairCut = a.HairCut,
                               // requiresLocation = a.RequiresLocation,
                                //lastUpdatedBy = (int) a.LastUpdatedBy,
                                companyId = a.CompanyId,
                                createdBy = a.CreatedBy,
                                dateTimeCreated = a.DateTimeCreated
                            }).ToList();
            return category;
        }

        public CollateralTypeViewModel GetCollateralTypesById(int typeId)
        {
            var category = (from a in context.tbl_Collateral_Type
                            where a.CollateralTypeId == typeId && a.Deleted == false
                            select new CollateralTypeViewModel
                            {
                               // collateralCategoryName = context.TblCollateralCategory.SingleOrDefault(c => c.CollateralCategoryId == a.CollateralCategoryId).CollateralCategoryName,
                               // collateralCategoryId = a.CollateralCategoryId,
                                collateralTypeName = a.CollateralTypeName,
                                collateralTypeId = a.CollateralTypeId,
                                dateTimeUpdated = a.DateTimeUpdated,
                                deleted = a.Deleted,
                                details = a.Details,
                              //  hairCut = a.HairCut,
                              //  requiresLocation = a.RequiresLocation,
                                //lastUpdatedBy = a.LastUpdatedBy.Value,
                                companyId = (short)a.CompanyId,
                                createdBy = a.CreatedBy,
                                dateTimeCreated = DateTime.UtcNow
                            }).SingleOrDefault();
            return category;
        }

        public CollateralTypeViewModel GetCollateralTypeByCategoryId(int categoryId)
        {
            var category = (from a in context.tbl_Collateral_Type 
                           // where a.CollateralCategoryId == categoryId && a.Deleted == false
                            select new CollateralTypeViewModel
                            {
                                //collateralCategoryName = context.TblCollateralCategory.SingleOrDefault(c => c.CollateralCategoryId == a.CollateralCategoryId).CollateralCategoryName,
                                //collateralCategoryId = a.CollateralCategoryId,
                                collateralTypeName = a.CollateralTypeName,
                                collateralTypeId = a.CollateralTypeId,
                                dateTimeUpdated = a.DateTimeUpdated,
                                deleted = a.Deleted,
                                details = a.Details,
                               // hairCut = a.HairCut,
                               // requiresLocation = a.RequiresLocation,
                                lastUpdatedBy = a.LastUpdatedBy.Value,
                                companyId = a.CompanyId,
                                createdBy = a.CreatedBy,
                                dateTimeCreated = a.DateTimeCreated
                            }).SingleOrDefault();
            return category;
        }

        public async Task<bool> UpdateCollateralTypes(int typeId, CollateralTypeViewModel entity)
        {
            var type = context.tbl_Collateral_Type.SingleOrDefault(c => c.CollateralTypeId == typeId);

            type.CompanyId = entity.companyId;
            type.DateTimeCreated = DateTime.UtcNow;
            type.DateTimeUpdated = entity.dateTimeUpdated;
            type.LastUpdatedBy = entity.lastUpdatedBy;
            type.CollateralTypeName = entity.collateralTypeName;
            //type.CollateralCategoryId = entity.collateralCategoryId;
            var respose = await context.SaveChangesAsync() != 0;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralTypeUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated Collateral Type: { entity.collateralTypeName } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return respose;
        }

        public async Task<bool> DeleteCollateralTypes(int typeId, CollateralTypeViewModel entity, UserInfo user)
        {
            var type = context.tbl_Collateral_Type.SingleOrDefault(c => c.CollateralTypeId == typeId);

            type.DateTimeCreated = DateTime.UtcNow;
            type.DeletedBy = entity.deletedBy;
            type.Deleted = true;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralTypeUpdated,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Collateral Type: { entity.collateralTypeName } ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> AddCollateralTypes(CollateralTypeViewModel entity)
        {
            var type = new tbl_Collateral_Type
            {
                CollateralTypeName = entity.collateralTypeName,
                CompanyId = entity.companyId,
                CreatedBy = (int)entity.createdBy,
                DateTimeCreated = DateTime.UtcNow,
               // CollateralCategoryId = entity.collateralCategoryId,
                Details = entity.details,
               // HairCut = entity.hairCut,
               // RequiresLocation = entity.requiresLocation
            };
            context.tbl_Collateral_Type.Add(type);
            var respose = await context.SaveChangesAsync() != 0;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralTypeAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Collateral Type: { entity.collateralTypeName } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return respose;
        }

        

        //#region Collateral Custom Fields
        //public async Task<bool> AddCollateralCustomFields(CollateralCustomFieldsViewModel entity)
        //{
        //    var custome = new TblCollateralCustomFields
        //    {
               
        //        CollateralTypeId = entity.collateralTypeId,
        //        CompanyId = entity.companyId,
        //        ControlType = entity.controlType,
        //        CreatedBy = entity.createdBy,
        //        DateTimeCreated = entity.dateTimeCreated,
        //        ItemOrder = entity.itemOrder,
        //        Required = entity.required,
        //        LabelName = entity.labelName
                 
        //    };   

        //    // Audit Section ---------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = entity.userBranchId,
        //        Detail = $"Added collateral requirement: { entity.labelName  }  to Collateral ",
        //        IPAddress = entity.userIPAddress,
        //        ApplicationDate = genSetup.GetApplicaionDate().Date,
        //        SystemDateTime = DateTime.UtcNow.Date,
        //        Url = entity.applicationUrl
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    return await context.SaveChangesAsync() != 0;

        //}

        //public async Task<bool> UpdateCollateralCustomFields(int collateralCustomFieldsId, CollateralCustomFieldsViewModel entity)
        //{
        //    var fields = context.TblCollateralCustomFields.Find(collateralCustomFieldsId);
        
        //    fields.CollateralTypeId = entity.collateralTypeId;
        //    fields.CompanyId = entity.companyId;
        //    fields.ControlType = entity.controlType;
        //    fields.LastUpdatedBy = entity.lastUpdatedBy;
        //    fields.DateTimeUpdated = entity.dateTimeUpdated;
        //    fields.ItemOrder = entity.itemOrder;
        //    fields.Required = entity.required;
        //    fields.LabelName = entity.labelName;            

        //    // Audit Section ---------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
        //        StaffId = entity.createdBy,
        //        BranchId = entity.userBranchId,
        //        Detail = $"Added collateral requirement: { entity.labelName  }  to Collateral ",
        //        IPAddress = entity.userIPAddress,
        //        ApplicationDate = genSetup.GetApplicaionDate().Date,
        //        SystemDateTime = DateTime.UtcNow.Date,
        //        Url = entity.applicationUrl
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    return await context.SaveChangesAsync() != 0;

        //}

        //public async Task<bool> DeleteCollateralCustomFields(int collateralCustomFieldsId, UserInfo user)
        //{
        //    var fields = context.TblCollateralCustomFields.Find(collateralCustomFieldsId);
                        
        //    fields.Deleted = fields.Deleted;
        //    fields.DeletedBy = fields.DeletedBy;
        //    fields.DateTimeDeleted = fields.DateTimeUpdated;           

        //    // Audit Section ---------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
        //        StaffId = user.createdBy,
        //        BranchId = (short)user.BranchId,
        //        Detail = $"Added collateral requirement: { fields.LabelName  }  to Collateral ",
        //        IPAddress = user.userIPAddress,
        //        ApplicationDate = genSetup.GetApplicaionDate().Date,
        //        SystemDateTime = DateTime.UtcNow.Date,
        //        Url =  user.applicationUrl
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    return await context.SaveChangesAsync() != 0;
        //}

        //public CollateralCustomFieldsViewModel CollateralCustomFieldsByCollateralCustomFieldsId(int collateralCustomFieldId, int companyId)
        //{
        //    return CollateralCustomFields(companyId).Where(c => c.collateralCustomFieldId == collateralCustomFieldId).SingleOrDefault();
        //}

        //public IEnumerable<CollateralCustomFieldsViewModel> CollateralCustomFieldsByCollateralTypeId(int collateralTypeId, int companyId)
        //{
        //    return CollateralCustomFields(companyId).Where(c => c.collateralTypeId == collateralTypeId);
        //}

        //public IEnumerable<CollateralCustomFieldsViewModel> GetCollateralCustomFields(  int companyId)
        //{
        //    return CollateralCustomFields(companyId);
        //}

        //private IEnumerable<CollateralCustomFieldsViewModel> CollateralCustomFields(int companyId)
        //{
        //    return context.TblCollateralCustomFields.Where(c => c.CompanyId == companyId).Select(c =>
        //        new CollateralCustomFieldsViewModel()
        //        {
        //            required = c.Required,
        //            collateralCustomFieldId = c.CollateralCustomFieldId,
        //            collateralTypeId = c.CollateralTypeId,
        //            companyId = c.CompanyId,
        //            collateralTypeName = context.TblCollateralType.SingleOrDefault(a => a.CollateralTypeId == c.CollateralTypeId).CollateralTypeName,
        //            controlType = c.ControlType,
        //            labelName = c.LabelName,
        //            itemOrder = c.ItemOrder,
        //            createdBy = c.CreatedBy,
        //            dateTimeCreated = c.DateTimeCreated
        //        });
        //}
        //#endregion Collateral Location
    }
}