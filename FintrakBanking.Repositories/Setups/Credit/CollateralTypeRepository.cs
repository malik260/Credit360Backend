using FintrakBanking.Common;
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
                        requireVisitation = m.REQUIREVISITATION,
                        details = m.DETAILS,
                        position = m.POSITION,
                        collateralClassificationId = m.COLLATERALCLASSIFICATIONID,

                    }).OrderBy(m=> m.position );
        }

        public IEnumerable<CollateralTypeViewModel> CollateralTypesByLoanApplication(int? applicationId)
        {
            //var list = context.TBL_COLLATERAL_TYPE_SUB
            //            .Select(m => new CollateralTypeViewModel
            //            {
            //                collateralTypeId = m.COLLATERALTYPEID,
            //                collateralTypeName = m.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
            //                chargeGLAccountId = m.TBL_COLLATERAL_TYPE.CHARGEGLACCOUNTID,
            //                requireInsurancePolicy = m.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
            //                details = m.TBL_COLLATERAL_TYPE.DETAILS,
            //                position = m.TBL_COLLATERAL_TYPE.POSITION,
            //                collateralSubTypeName = m.COLLATERALSUBTYPENAME,
            //                collateralSubTypeId = m.COLLATERALSUBTYPEID,

            //            })
            //            .Distinct()
            //            .OrderBy(m => m.position);

            var list = context.TBL_COLLATERAL_TYPE
                       .Select(m => new CollateralTypeViewModel
                       {
                           collateralTypeId = m.COLLATERALTYPEID,
                           collateralTypeName = m.COLLATERALTYPENAME,
                           requireInsurancePolicy = m.REQUIREINSURANCEPOLICY,
                           requireVisitation = m.REQUIREVISITATION,
                           collateralClassificationId = m.COLLATERALCLASSIFICATIONID

                       }).ToList();
                      // .Distinct();
                      ///.OrderBy(m => m.position);

            if (applicationId == null || applicationId == 0) { return list; }

            var productIds = context.TBL_LOAN_APPLICATION_DETAIL
                .Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x=>x.PROPOSEDPRODUCTID)
                .Distinct();
            if (productIds == null || productIds.Count()<1)
            {
                 productIds = context.TBL_LMSR_APPLICATION_DETAIL
                    .Where(x => x.LOANREVIEWAPPLICATIONID == applicationId)
                    .Select(x => x.PRODUCTID)
                    .Distinct();
            }
            var typeIds = context.TBL_PRODUCT_COLLATERALTYPE.Where(x => productIds.Contains(x.PRODUCTID))
                .Select(x => x.COLLATERALTYPEID)
                .Distinct();

            var data = list.Where(x => typeIds.Contains((short)x.collateralTypeId)).ToList();

            return data;
        }
        public IEnumerable<CollateralDocumentTypeViewModel> GetCollateralDocumentTypes(int id)
        {
            //var documentType = context.TBL_COLLATERAL_DOCUMENT_TYPE.Where(x => x.COLLATERALTYPEID == id).ToList();

            var documentType = (from m in context.TBL_COLLATERAL_DOCUMENT_TYPE
                                where m.COLLATERALTYPEID == id
                       select new CollateralDocumentTypeViewModel
                       {
                           collateralTypeId = m.COLLATERALTYPEID,
                           documentTypeId = m.DOCUMENTTYPEID,
                           documentType = m.DOCUMENTTYPENAME,
                          
                       }).ToList();


            return documentType;
        }
        public bool AddCollateralDocumentType(CollateralDocumentTypeViewModel entity)
        {
            bool output = false;
            var collateralType = context.TBL_COLLATERAL_TYPE.Where(x => x.COLLATERALTYPEID == entity.collateralTypeId).FirstOrDefault();
            if(entity.documentTypeId==0)
            {
                var docType = new TBL_COLLATERAL_DOCUMENT_TYPE
                {
                    COLLATERALTYPEID = entity.collateralTypeId,
                    DOCUMENTTYPENAME = entity.documentType,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now.Date,
                };

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CollateralDocumentType,
                    STAFFID = entity.createdBy,
                    BRANCHID = entity.userBranchId,
                    DETAIL = $"Added collateral Document Type: { entity.documentType  }  Created for {collateralType.COLLATERALTYPENAME} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    APPLICATIONDATE = genSetup.GetApplicationDate().Date,
                    SYSTEMDATETIME = DateTime.Now.Date,
                    URL = entity.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                context.TBL_COLLATERAL_DOCUMENT_TYPE.Add(docType);
                this.auditTrail.AddAuditTrail(audit);
                output = context.SaveChanges() > 0;
            }
            else
            {
                TBL_COLLATERAL_DOCUMENT_TYPE docType = new TBL_COLLATERAL_DOCUMENT_TYPE();

                docType = context.TBL_COLLATERAL_DOCUMENT_TYPE.Where(x => x.DOCUMENTTYPEID == entity.documentTypeId).FirstOrDefault();
                docType.DOCUMENTTYPENAME = entity.documentType;
                docType.LASTUPDATEDBY = entity.createdBy;
                docType.DATETIMEUPDATED = DateTime.Now.Date;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CollateralDocumentType,
                    STAFFID = entity.createdBy,
                    BRANCHID = entity.userBranchId,
                    DETAIL = $"Updated collateral Document Type: { entity.documentType  }  Created for {collateralType.COLLATERALTYPENAME} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    APPLICATIONDATE = genSetup.GetApplicationDate().Date,
                    SYSTEMDATETIME = DateTime.Now.Date,
                    URL = entity.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                //context.TBL_COLLATERAL_DOCUMENT_TYPE.Add(docType);
                this.auditTrail.AddAuditTrail(audit);
                output = context.SaveChanges() > 0;
            }

            return output;
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
            type.REQUIREVISITATION = entity.requireVisitation;
            type.COLLATERALCLASSIFICATIONID = entity.collateralClassificationId;
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Collateral Type: { entity.collateralTypeName } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            var respose = await context.SaveChangesAsync() != 0;
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
                        isLocationBased = m.ISLOCATIONBASED,
                        allowSharing=m.ALLOWSHARING,
                        createdBy = m.CREATEDBY,
                        visitationCycle = m.VISITATIONCYCLE,
                        collateralType = context.TBL_COLLATERAL_TYPE.Where(c => c.COLLATERALTYPEID == m.COLLATERALTYPEID).FirstOrDefault().COLLATERALTYPENAME,
                    }).ToList();
        }
       
        #region Collateral SubTypes By ID
        public CollateralSubTypeViewModel CollateralSubType(int Id)
        {
            return (from m in context.TBL_COLLATERAL_TYPE_SUB
                    where m.COLLATERALSUBTYPEID == Id
                    select new CollateralSubTypeViewModel
                    {
                        haircut = m.HAIRCUT,
                        revaluationDuration = m.REVALUATIONDURATION,
                        isLocationBased = m.ISLOCATIONBASED,
                        allowSharing = m.ALLOWSHARING,
                        visitationCycle = m.VISITATIONCYCLE,
                        isGPScollateralType = m.ISGPSCOORDINATESCOLLATERALTYPE

                    }).FirstOrDefault();
        }
        #endregion End od Collateral SubType by ID
        public IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypes()
        {
            return (from m in context.TBL_COLLATERAL_TYPE_SUB
                    join t in context.TBL_COLLATERAL_TYPE 
                    on m.COLLATERALTYPEID equals t.COLLATERALTYPEID
                    select new CollateralSubTypeViewModel
                    {
                        collateralSubTypeId = m.COLLATERALSUBTYPEID,
                        collateralTypeId = m.COLLATERALTYPEID,
                        collateralSubTypeName = m.COLLATERALSUBTYPENAME,
                        haircut = m.HAIRCUT,  
                        revaluationDuration = m.REVALUATIONDURATION,
                        isLocationBased = m.ISLOCATIONBASED,
                        allowSharing = m.ALLOWSHARING,
                        collateralTypeName = t.COLLATERALTYPENAME,
                        visitationCycle = m.VISITATIONCYCLE,
                        isGPScollateralType = m.ISGPSCOORDINATESCOLLATERALTYPE
                    }).ToList();
        }
        
        public IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            return CollateralSubType().Where(x => x.collateralTypeId == collateralTypeId);
        }


        public bool UpdateCollateralSubTypes(int subTypeId, CollateralSubTypeViewModel entity)
        {
            if (entity.GPScollateralType == "0")
            {
                entity.isGPScollateralType = false;
            }
            else { entity.isGPScollateralType = true; }
            var subType = context.TBL_COLLATERAL_TYPE_SUB.Find(subTypeId);

            subType.COLLATERALTYPEID = entity.collateralTypeId;
            subType.COLLATERALSUBTYPENAME = entity.collateralSubTypeName;
            subType.HAIRCUT = entity.haircut;
            subType.REVALUATIONDURATION = entity.revaluationDuration;
            subType.DATETIMEUPDATED = genSetup.GetApplicationDate();
            subType.LASTUPDATEDBY = entity.lastUpdatedBy;
            subType.ISLOCATIONBASED = entity.isLocationBased;
            subType.ALLOWSHARING = entity.allowSharing;
            subType.VISITATIONCYCLE = entity.visitationCycle;
            subType.ISGPSCOORDINATESCOLLATERALTYPE = entity.isGPScollateralType;


            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Collateral Type: { entity.collateralSubTypeName } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            var respose = context.SaveChanges() >= 0;
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public bool AddCollateralSubTypes(CollateralSubTypeViewModel entity)
        {
            if (entity.GPScollateralType == "0")
            {
                entity.isGPScollateralType = false;
            }
            else { entity.isGPScollateralType = true; }
            var type = new TBL_COLLATERAL_TYPE_SUB
            {
                COLLATERALSUBTYPENAME = entity.collateralSubTypeName,
                COLLATERALTYPEID = entity.collateralTypeId,
                HAIRCUT = entity.haircut,
                REVALUATIONDURATION = entity.revaluationDuration,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                ISLOCATIONBASED = entity.isLocationBased,
                ALLOWSHARING = entity.allowSharing,
                ISGPSCOORDINATESCOLLATERALTYPE = entity.isGPScollateralType
            };
            context.TBL_COLLATERAL_TYPE_SUB.Add(type);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Collateral Type sub: { entity.collateralSubTypeName } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            var respose = context.SaveChanges() != 0;

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
