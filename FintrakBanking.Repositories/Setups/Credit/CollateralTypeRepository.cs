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
            return (from m in context.TBL_COLLATERAL_TYPE
                    select new CollateralTypeViewModel
                    {
                        collateralTypeId = m.COLLATERALTYPEID,
                        collateralTypeName = m.COLLATERALTYPENAME,
                        chargeGLAccountId = m.CHARGEGLACCOUNTID,
                        requireInsurancePolicy = m.REQUIREINSURANCEPOLICY,
                        details = m.DETAILS,
                         position = m.POSITION 
                    }).OrderBy(m=> m.position );
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
            var type = context.TBL_COLLATERAL_TYPE.SingleOrDefault(c => c.COLLATERALTYPEID == typeId);

            type.CHARGEGLACCOUNTID = (int)entity.chargeGLAccountId;
            type.REQUIREINSURANCEPOLICY = entity.requireInsurancePolicy;
            type.DATETIMEUPDATED = genSetup.GetApplicationDate();
            type.LASTUPDATEDBY = entity.lastUpdatedBy;
            var respose = await context.SaveChangesAsync() != 0;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Collateral Type: { entity.collateralTypeName } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return respose;
        }
        #endregion End of Collateral Type


        #region Collateral SubTypes
        private IEnumerable<CollateralSubTypeViewModel> CollateralSubType()
        {
            return (from m in context.TBL_COLLATERAL_TYPE_SUB
                    select new CollateralSubTypeViewModel
                    {
                        collateralSubTypeId = m.COLLATERALSUBTYPEID,
                        collateralTypeId = m.COLLATERALTYPEID,
                        collateralSubTypeName = m.COLLATERALSUBTYPENAME,
                        haircut = m.HAIRCUT,
                        revaluationDuration = m.REVALUATIONDURATION,
                        dateTimeCreated = m.DATETIMECREATED.Date,
                        createdBy = m.CREATEDBY
                    }).ToList();
        }

        public IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypes()
        {
            return (from m in context.TBL_COLLATERAL_TYPE_SUB
                    select new CollateralSubTypeViewModel
                    {
                        collateralSubTypeId = m.COLLATERALSUBTYPEID,
                        collateralTypeId = m.COLLATERALTYPEID,
                        collateralSubTypeName = m.COLLATERALSUBTYPENAME,
                        haircut = m.HAIRCUT,
                        revaluationDuration = m.REVALUATIONDURATION,
                    }).ToList();
        }
        
        public IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            return CollateralSubType().Where(x => x.collateralTypeId == collateralTypeId);
        }


        public async Task<bool> UpdateCollateralSubTypes(int subTypeId, CollateralSubTypeViewModel entity)
        {
            var subType = context.TBL_COLLATERAL_TYPE_SUB.Find(subTypeId);

            subType.COLLATERALTYPEID = entity.collateralTypeId;
            subType.COLLATERALSUBTYPENAME = entity.collateralSubTypeName;
            subType.HAIRCUT = entity.haircut;
            subType.REVALUATIONDURATION = entity.revaluationDuration;
            subType.DATETIMEUPDATED = genSetup.GetApplicationDate();
            subType.LASTUPDATEDBY = entity.lastUpdatedBy;
            var respose = await context.SaveChangesAsync() != 0;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Collateral Type: { entity.collateralSubTypeName } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return respose;
        }

        public async Task<bool> DeleteCollateralSubTypes(int subTypeId, CollateralSubTypeViewModel entity, UserInfo user)
        {
            var type = context.TBL_COLLATERAL_TYPE_SUB.Find(subTypeId);

            type.DATETIMECREATED = DateTime.Now;
            type.DELETEDBY = entity.deletedBy;
            type.DELETED = true;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Collateral Sub Type: { entity.collateralSubTypeName } ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> AddCollateralSubTypes(CollateralSubTypeViewModel entity)
        {
            var type = new TBL_COLLATERAL_TYPE_SUB
            {
                COLLATERALSUBTYPENAME = entity.collateralSubTypeName,
                COLLATERALTYPEID = entity.collateralTypeId,
                HAIRCUT = entity.haircut,
                REVALUATIONDURATION = entity.revaluationDuration,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
            };
            context.TBL_COLLATERAL_TYPE_SUB.Add(type);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Collateral Type sub: { entity.collateralSubTypeName } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            var respose = await context.SaveChangesAsync() != 0;

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