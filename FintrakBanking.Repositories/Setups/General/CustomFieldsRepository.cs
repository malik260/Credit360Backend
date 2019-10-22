using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(ICustomFieldsRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CustomFieldsRepository : ICustomFieldsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IProductRepository product;
        public CustomFieldsRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                        IProductRepository _product)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            product = _product;
        }

        #region   Custom Fields

        //ossy

        public async Task<bool> AddCustomField(AddCustomFieldViewModel model)
        {
            var custom = new TBL_CUSTOM_FIELDS
            {
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                CONTROLTYPE = model.controlType,
                LABELNAME = model.labelName,
                HOSTPAGEID = model.hostPageId,
                ITEMORDER = model.itemOrder,
                REQUIRED = model.required
            };

            context.TBL_CUSTOM_FIELDS.Add(custom);

            //foreach (var options in model.customFieldOption)
            //{
            //    var option = new TblCustomFieldOption
            //    {
            //        OptionsKey = options.optionsKey,
            //        OptionsValue = options.optionsValue,
            //        CustomFieldId = options.customFieldId,
            //    };
            //    context.TblCustomFieldOption.Add(option);
            //}

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomFieldAdd,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added the following custom field : { model.labelName} to  {context.TBL_CUSTOM_HOSTPAGE.Find(model.hostPageId).HOSTPAGE } form ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------


            return await context.SaveChangesAsync() != 0;
        }

        // update

        public async Task<bool> UpdateCustomField(AddCustomFieldViewModel model, int id)
        {
            bool result = false;

            var custom = context.TBL_CUSTOM_FIELDS.Find(id);
            custom.CONTROLTYPE = model.controlType;
            custom.LABELNAME = model.labelName;
            custom.HOSTPAGEID = model.hostPageId;
            custom.ITEMORDER = model.itemOrder;
            custom.REQUIRED = model.required;
            custom.LASTUPDATEDBY = model.lastUpdatedBy;
            custom.DATETIMEUPDATED = genSetup.GetApplicationDate().Date;
            //foreach (var option in model.customFieldOption)
            //{
            //    var options = context.TblCustomFieldOption.SingleOrDefault(c => c.CustomFieldOptionsId == option.customFieldOptionsId);
            //    options.OptionsKey = option.optionsKey;
            //    options.OptionsValue = option.optionsValue;
            //    options.CustomFieldId = option.customFieldId;
            //}
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomFieldUpdate,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"updated the following custom field : { model.labelName} to  {context.TBL_CUSTOM_HOSTPAGE.Find(model.hostPageId).HOSTPAGE } form ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            result = await context.SaveChangesAsync() != 0;

            return result;
        }

        //ossy








        public async Task<bool> AddCustomFields(List<CustomFieldViewModel> listEntity)
        {

            foreach (var entity in listEntity)
            {
                var custom = new TBL_CUSTOM_FIELDS
                {
                    ACTEDONBY = entity.actedOnBy,
                    APPROVALSTATUS = entity.approvalStatus,
                    COMPANYID = entity.companyId,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = entity.dateTimeCreated,
                    CONTROLTYPE = entity.controlType,
                    LABELNAME = entity.labelName,
                    CONTROLKEY = entity.controlKey,
                    HOSTPAGEID = entity.hostPageId,
                    ITEMORDER = entity.itemOrder,
                    REQUIRED = entity.required
                };

                context.TBL_CUSTOM_FIELDS.Add(custom);

                foreach (var options in entity.customFieldOption)
                {
                    var option = new TBL_CUSTOM_FIELD_OPTION
                    {
                        OPTIONSKEY = options.optionsKey,
                        OPTIONSVALUE = options.optionsValue,
                        CUSTOMFIELDID = options.customFieldId,
                    };
                    context.TBL_CUSTOM_FIELD_OPTION.Add(option);
                }

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomFieldAdd,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"Added the following custom field : { entity.labelName} to  {context.TBL_CUSTOM_HOSTPAGE.Find(entity.hostPageId).HOSTPAGE } form ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------
            }


            return await context.SaveChangesAsync() != 0;
        }

        IEnumerable<CustomFieldViewModel> CustomFields(int companyId)
        {
            return context.TBL_CUSTOM_FIELDS .Where(c => c.COMPANYID == companyId && c.DELETED == false).Select(c => new CustomFieldViewModel()
            {
                actedOnBy = c.ACTEDONBY,
                approvalStatus = c.APPROVALSTATUS,
                controlType = c.CONTROLTYPE,
                labelName = c.LABELNAME,
                controlKey = c.CONTROLKEY,
                companyId = c.COMPANYID,
                customFieldId = c.CUSTOMFIELDID,
                hostPageId = c.HOSTPAGEID,
                itemOrder = c.ITEMORDER,
                required = c.REQUIRED,
                dateTimeCreated = c.DATETIMECREATED
            });
        }

        public IEnumerable<CustomFieldViewModel> CustomFieldsByHostPageId(int hostPageId, int companyId)
        {
            return CustomFields(companyId).Where(c => c.hostPageId == hostPageId);
        }

        public async Task<bool> DeleteCustomFields(List<CustomFieldViewModel> customFields, UserInfo user)
        {
            bool result = false;
            for (int i = 0; customFields.Count > i; i++)
            {
                var custom = context.TBL_CUSTOM_FIELDS.Find(customFields[i].customFieldId);
                custom.DELETED = true;
                custom.DATETIMEDELETED = genSetup.GetApplicationDate().Date;
                custom.DELETEDBY = user.staffId;
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomFieldDelete,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"updated the following custom field : { custom.LABELNAME} to  {context.TBL_CUSTOM_HOSTPAGE.Find(custom.HOSTPAGEID).HOSTPAGE } form ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------
                result = await context.SaveChangesAsync() != 0;

            }
            return result;
        }

        public async Task<bool> UpdateCustomFields(List<CustomFieldViewModel> listEntity)
        {
            bool result = false;
            foreach (var entity in listEntity)
            {
                var custom = context.TBL_CUSTOM_FIELDS .Find(entity.customFieldId);
                custom.ACTEDONBY = entity.actedOnBy;
                custom.APPROVALSTATUS = entity.approvalStatus;
                custom.COMPANYID = entity.companyId;
                custom.CREATEDBY = entity.createdBy;
                custom.CONTROLKEY = entity.controlKey;
                custom.CONTROLTYPE = entity.controlType;
                custom.LABELNAME = entity.labelName;
                custom.HOSTPAGEID = entity.hostPageId;
                custom.ITEMORDER = entity.itemOrder;
                custom.REQUIRED = entity.required;
                custom.LASTUPDATEDBY = entity.lastUpdatedBy;
                custom.DATETIMEUPDATED = genSetup.GetApplicationDate().Date;
                foreach (var option in entity.customFieldOption)
                {
                    var options = context.TBL_CUSTOM_FIELD_OPTION .SingleOrDefault(c => c.CUSTOMFIELDOPTIONSID == option.customFieldOptionsId);
                    options.OPTIONSKEY = option.optionsKey;
                    options.OPTIONSVALUE = option.optionsValue;
                    options.CUSTOMFIELDID = option.customFieldId;
                }
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomFieldUpdate,
                    STAFFID = entity.lastUpdatedBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"updated the following custom field : { entity.labelName} to  {context.TBL_CUSTOM_HOSTPAGE.Find(entity.hostPageId).HOSTPAGE } form ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------
                result = await context.SaveChangesAsync() != 0;
            }

            return result;
        }
        #endregion   Custom Fields

         

            #region   Custom Fields Data

            public async Task<bool> AddCustomFieldsData(List<CustomFieldsDataViewModel> listEntity)
        {
            foreach (var entity in listEntity)
            {
                if (entity.isUpload)
                {
                    var fieldDetailsUpload = new TBL_CUSTOM_FIELD_DATA_UPLOAD
                    {
                        CUSTOMFIELDDATAUPLOAD = entity.customFieldDataUpload,
                        CUSTOMFIELDSDATAID = entity.customFieldsDataId,
                        DATETIMECREATED = genSetup.GetApplicationDate().Date,
                        CREATEDBY = entity.createdBy
                    };
                    context.TBL_CUSTOM_FIELD_DATA_UPLOAD.Add(fieldDetailsUpload);
                }
                var fieldDetails = new TBL_CUSTOM_FIELDS_DATA
                {
                    CREATEDBY = entity.createdBy,

                    OWNERID = entity.ownerId,
                    CUSTOMFIELDID = entity.customFieldId,
                    DATADETAILS = entity.isUpload ? Guid.NewGuid().ToString() : entity.dataDetails,
                    DATETIMECREATED = genSetup.GetApplicationDate().Date 
                };
                context.TBL_CUSTOM_FIELDS_DATA.Add(fieldDetails);

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomFieldDatadAdd,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"Added the following custom field : { entity.labelName} to  {context.TBL_CUSTOM_HOSTPAGE.Find(entity.hostPageId).HOSTPAGE } form ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                this.auditTrail.AddAuditTrail(audit);
              
            }
            return await context.SaveChangesAsync() != 0;
        }

        private IEnumerable<CustomFieldsDataViewModel> CustomFieldsData(int hostPageId, int customerId, int companyId)
        {
            var CustomFieldData = context.TBL_CUSTOM_FIELDS_DATA;
            var customs = (from d in context.TBL_CUSTOM_FIELDS
                           join s in context.TBL_CUSTOM_HOSTPAGE on d.HOSTPAGEID equals s.HOSTPAGEID
                           where s.HOSTPAGEID == hostPageId && d.COMPANYID == companyId && d.DELETED == false
                           select new CustomFieldsDataViewModel
                           {
                               controlType = d.CONTROLTYPE,
                               customFieldId = d.CUSTOMFIELDID,
                               itemOrder = d.ITEMORDER,
                               labelName = d.LABELNAME,
                               hostPageId = s.HOSTPAGEID,
                               parentHostPageId = s.PARENTHOSTPAGEID,
                               hostPage = s.HOSTPAGE,
                               required = d.REQUIRED,
                               isUpload = d.ISUPLOAD,
                               controlKey =d.CONTROLKEY,
                               //customFieldOption = context.TblCustomFieldOption.Where(h => h.CustomFieldId == d.CustomFieldId)
                               //.Select(h => new CustomFieldOptionViewModel()
                               //{
                               //    customFieldOptionsId = h.CustomFieldOptionsId,
                               //    optionsKey = h.OptionsKey,
                               //    optionsValue = h.OptionsValue
                               //}).ToList(),
                           }).ToList();

            if (customs.Any())
            {

                foreach (var custom in customs)
                {
                    var check = CustomFieldData.Where(c => c.CUSTOMFIELDID == custom.customFieldId && c.OWNERID == customerId);
                    if (check.Count() > 0)
                    {
                        var data = check.First();
                        custom.customFieldDataUpload = custom.isUpload ? context.TBL_CUSTOM_FIELD_DATA_UPLOAD.FirstOrDefault(d => d.CUSTOMFIELDSDATAID == data.CUSTOMFIELDSDATAID).CUSTOMFIELDDATAUPLOAD : null;
                        custom.customFieldsDataId = data.CUSTOMFIELDSDATAID;
                        custom.dataDetails = data.DATADETAILS;
                        custom.isUpload = custom.isUpload;
                        custom.ownerId = data.OWNERID;

                    }
                }
            }
            return customs.ToList();
        }

        public async  Task<bool> DeleteCustomFieldsData(List<CustomFieldsDataViewModel> listEntity, UserInfo user)
        {
            foreach (var entity in listEntity) {
                var field = context.TBL_CUSTOM_FIELDS_DATA.Find(entity.customFieldsDataId);
                field.DELETED = true;
                field.DELETEDBY = user.staffId;
                field.DATETIMEDELETED = genSetup.GetApplicationDate().Date;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomFieldDataDelete,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"Deleted a custome field data for : { entity.labelName} from  {context.TBL_CUSTOM_HOSTPAGE.Find(entity.hostPageId).HOSTPAGE }  ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                this.auditTrail.AddAuditTrail(audit);
            }
            return await  context.SaveChangesAsync() != 0;
        }

        public async Task<bool> UpdateCustomFieldsData(List<CustomFieldsDataViewModel> listEntity)
        {
            bool response = false;
            foreach (var entity in listEntity)
            {
                if (entity.isUpload)
                {
                    var fieldDetailsUpload = context.TBL_CUSTOM_FIELD_DATA_UPLOAD.Find(entity.customFieldDataUpload);
                    fieldDetailsUpload.CUSTOMFIELDDATAUPLOAD = entity.customFieldDataUpload;
                    fieldDetailsUpload.CUSTOMFIELDSDATAID = entity.customFieldsDataId;
                    fieldDetailsUpload.DATETIMECREATED = genSetup.GetApplicationDate().Date;
                    fieldDetailsUpload.CREATEDBY = entity.createdBy;                      
                }
                var fieldDetails = context.TBL_CUSTOM_FIELDS_DATA.Find(entity.customFieldsDataId);
                fieldDetails.CREATEDBY = entity.createdBy;
                fieldDetails.OWNERID = entity.ownerId;
                fieldDetails.CUSTOMFIELDID = entity.customFieldId;
                fieldDetails.DATADETAILS = entity.isUpload ? Guid.NewGuid().ToString() : entity.dataDetails;
                fieldDetails.DATETIMECREATED = genSetup.GetApplicationDate().Date;
              

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomFieldDatadAdd,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"Added the following custom field : { entity.labelName} to  {context.TBL_CUSTOM_HOSTPAGE.Find(entity.hostPageId).HOSTPAGE } form ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                this.auditTrail.AddAuditTrail(audit);
                response = await context.SaveChangesAsync() != 0;
            }
            return response;
        }

        public IEnumerable<CustomFieldsDataViewModel> GetCustomFieldsDataByHostPage(int hostPageId,int customerId, int companyId)
        {
            return CustomFieldsData(hostPageId, companyId, customerId);
        }
        
        #endregion   Custom Fields Data

        #region   host page

        private IEnumerable<HostPageViewModel> HostPage()
        {
            return context.TBL_CUSTOM_HOSTPAGE.Select(c => new HostPageViewModel()
            {
                hostPage = c.HOSTPAGE,
                hostPageId = c.HOSTPAGEID,
                parentHostPageId = c.PARENTHOSTPAGEID
            });
        }

        public IEnumerable<HostPageViewModel> GetHostPages()
        {
            return HostPage();
        }

        public IEnumerable<HostPageViewModel> GetHostPagesChildrenOnly(int parentHostPageId)
        {
            return HostPage().Where(c=> c.parentHostPageId == parentHostPageId);
        }

        public IEnumerable<HostPageViewModel> GetHostPagesParentOnly()
        {
            return HostPage().Where(c=> c.parentHostPageId == 0);
        }

        public IEnumerable<HostPageViewModel> GetHostPages(int parentHostPageId)
        {
            return HostPage().Where(c => c.parentHostPageId == parentHostPageId);
        }

        #endregion   host page

    }
}
