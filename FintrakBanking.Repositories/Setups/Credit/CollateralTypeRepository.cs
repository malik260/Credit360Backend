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

        #region Collateral Type
        private IEnumerable<CollateralTypeViewModel>  CollateralTypes()
        {
            return (from m in context.tbl_Collateral_Type
                    select new CollateralTypeViewModel
                    {
                        collateralTypeId = m.CollateralTypeId,
                        collateralTypeName = m.CollateralTypeName,
                        chargeGLAccountId = m.ChargeGLAccountId,
                        requireInsurancePolicy = m.RequireInsurancePolicy,
                        details = m.Details
                    });
        }

        public IEnumerable<CollateralTypeViewModel> GetCollateralTypes()
        {
            return CollateralTypes();
        }

        public CollateralTypeViewModel GetCollateralTypesById(int typeId)
        {
            return CollateralTypes().Where(a => a.collateralTypeId == typeId).SingleOrDefault();
        }

        public async Task<bool> UpdateCollateralTypes(int typeId, CollateralTypeViewModel entity)
        {
            var type = context.tbl_Collateral_Type.SingleOrDefault(c => c.CollateralTypeId == typeId);

            type.ChargeGLAccountId = entity.chargeGLAccountId;
            type.RequireInsurancePolicy = entity.requireInsurancePolicy;
            type.DateTimeUpdated = genSetup.GetApplicationDate();
            type.LastUpdatedBy = entity.lastUpdatedBy;
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
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return respose;
        }
        #endregion End of Collateral Type


        #region Collateral SubTypes
        private IEnumerable<CollateralSubTypeViewModel> CollateralSubType()
        {
            return (from m in context.tbl_Collateral_Type_Sub
                    select new CollateralSubTypeViewModel
                    {
                        collateralSubTypeId = m.CollateralSubTypeId,
                        collateralTypeId = m.CollateralTypeId,
                        collateralSubTypeName = m.CollateralSubTypeName,
                        haircut = m.Haircut,
                        revaluationDuration = m.RevaluationDuration,
                        dateTimeCreated = m.DateTimeCreated.Date,
                        createdBy = m.CreatedBy
                    }).ToList();
        }

        public IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypes()
        {
            return (from m in context.tbl_Collateral_Type_Sub
                    select new CollateralSubTypeViewModel
                    {
                        collateralSubTypeId = m.CollateralSubTypeId,
                        collateralTypeId = m.CollateralTypeId,
                        collateralSubTypeName = m.CollateralSubTypeName,
                        haircut = m.Haircut,
                        revaluationDuration = m.RevaluationDuration,
                    }).ToList();
        }

        public IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            return CollateralSubType().Where(x => x.collateralTypeId == collateralTypeId);
        }

        public async Task<bool> UpdateCollateralSubTypes(int subTypeId, CollateralSubTypeViewModel entity)
        {
            var subType = context.tbl_Collateral_Type_Sub.Find(subTypeId);

            subType.CollateralTypeId = entity.collateralTypeId;
            subType.CollateralSubTypeName = entity.collateralSubTypeName;
            subType.DateTimeUpdated = genSetup.GetApplicationDate();
            subType.LastUpdatedBy = entity.lastUpdatedBy;
            var respose = await context.SaveChangesAsync() != 0;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralTypeUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated Collateral Type: { entity.collateralSubTypeName } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return respose;
        }

        public async Task<bool> DeleteCollateralSubTypes(int subTypeId, CollateralSubTypeViewModel entity, UserInfo user)
        {
            var type = context.tbl_Collateral_Type_Sub.Find(subTypeId);

            type.DateTimeCreated = DateTime.Now;
            type.DeletedBy = entity.deletedBy;
            type.Deleted = true;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralTypeUpdated,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Collateral Sub Type: { entity.collateralSubTypeName } ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> AddCollateralSubTypes(CollateralSubTypeViewModel entity)
        {
            var type = new tbl_Collateral_Type_Sub
            {
                CollateralSubTypeName = entity.collateralSubTypeName,
                CollateralTypeId = entity.collateralTypeId,
                Haircut = entity.haircut,
                RevaluationDuration = entity.revaluationDuration
            };
            context.tbl_Collateral_Type_Sub.Add(type);
            var respose = await context.SaveChangesAsync() != 0;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralTypeAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Collateral Type sub: { entity.collateralSubTypeName } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return respose;
        }
        #endregion End od Collateral SubType


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
        //        SystemDateTime = DateTime.Now.Date,
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
        //        SystemDateTime = DateTime.Now.Date,
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
        //        SystemDateTime = DateTime.Now.Date,
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