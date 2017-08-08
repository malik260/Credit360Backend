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
            var custom = new tbl_Custom_Fields
            {
                CompanyId = model.companyId,
                CreatedBy = model.createdBy,
                DateTimeCreated = DateTime.Now,
                ControlType = model.controlType,
                LabelName = model.labelName,
                HostPageId = model.hostPageId,
                ItemOrder = model.itemOrder,
                Required = model.required
            };

            context.tbl_Custom_Fields.Add(custom);

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
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomFieldAdd,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added the following custom field : { model.labelName} to  {context.tbl_Custom_HostPage.Find(model.hostPageId).HostPage } form ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------


            return await context.SaveChangesAsync() != 0;
        }

        // update

        public async Task<bool> UpdateCustomField(AddCustomFieldViewModel model, int id)
        {
            bool result = false;

            var custom = context.tbl_Custom_Fields.Find(id);
            custom.ControlType = model.controlType;
            custom.LabelName = model.labelName;
            custom.HostPageId = model.hostPageId;
            custom.ItemOrder = model.itemOrder;
            custom.Required = model.required;
            custom.LastUpdatedBy = model.lastUpdatedBy;
            custom.DateTimeUpdated = genSetup.GetApplicationDate().Date;
            //foreach (var option in model.customFieldOption)
            //{
            //    var options = context.TblCustomFieldOption.SingleOrDefault(c => c.CustomFieldOptionsId == option.customFieldOptionsId);
            //    options.OptionsKey = option.optionsKey;
            //    options.OptionsValue = option.optionsValue;
            //    options.CustomFieldId = option.customFieldId;
            //}
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomFieldUpdate,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"updated the following custom field : { model.labelName} to  {context.tbl_Custom_HostPage.Find(model.hostPageId).HostPage } form ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
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
                var custom = new tbl_Custom_Fields
                {
                    ActedOnBy = entity.actedOnBy,
                    ApprovalStatus = entity.approvalStatus,
                    CompanyId = entity.companyId,
                    CreatedBy = entity.createdBy,
                    DateTimeCreated = entity.dateTimeCreated,
                    ControlType = entity.controlType,
                    LabelName = entity.labelName,
                    ControlKey = entity.controlKey,
                    HostPageId = entity.hostPageId,
                    ItemOrder = entity.itemOrder,
                    Required = entity.required
                };

                context.tbl_Custom_Fields.Add(custom);

                foreach (var options in entity.customFieldOption)
                {
                    var option = new tbl_Custom_Field_Option
                    {
                        OptionsKey = options.optionsKey,
                        OptionsValue = options.optionsValue,
                        CustomFieldId = options.customFieldId,
                    };
                    context.tbl_Custom_Field_Option.Add(option);
                }

                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CustomFieldAdd,
                    StaffId = entity.createdBy,
                    BranchId = (short)entity.userBranchId,
                    Detail = $"Added the following custom field : { entity.labelName} to  {context.tbl_Custom_HostPage.Find(entity.hostPageId).HostPage } form ",
                    IPAddress = entity.userIPAddress,
                    Url = entity.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------
            }


            return await context.SaveChangesAsync() != 0;
        }

        IEnumerable<CustomFieldViewModel> CustomFields(int companyId)
        {
            return context.tbl_Custom_Fields .Where(c => c.CompanyId == companyId && c.Deleted == false).Select(c => new CustomFieldViewModel()
            {
                actedOnBy = c.ActedOnBy,
                approvalStatus = c.ApprovalStatus,
                controlType = c.ControlType,
                labelName = c.LabelName,
                controlKey = c.ControlKey,
                companyId = c.CompanyId,
                customFieldId = c.CustomFieldId,
                hostPageId = c.HostPageId,
                itemOrder = c.ItemOrder,
                required = c.Required,
                dateTimeCreated = c.DateTimeCreated
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
                var custom = context.tbl_Custom_Fields.Find(customFields[i].customFieldId);
                custom.Deleted = true;
                custom.DateTimeDeleted = genSetup.GetApplicationDate().Date;
                custom.DeletedBy = user.staffId;
                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CustomFieldDelete,
                    StaffId = user.staffId,
                    BranchId = (short)user.BranchId,
                    Detail = $"updated the following custom field : { custom.LabelName} to  {context.tbl_Custom_HostPage.Find(custom.HostPageId).HostPage } form ",
                    IPAddress = user.userIPAddress,
                    Url = user.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
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
                var custom = context.tbl_Custom_Fields .Find(entity.customFieldId);
                custom.ActedOnBy = entity.actedOnBy;
                custom.ApprovalStatus = entity.approvalStatus;
                custom.CompanyId = entity.companyId;
                custom.CreatedBy = entity.createdBy;
                custom.ControlKey = entity.controlKey;
                custom.ControlType = entity.controlType;
                custom.LabelName = entity.labelName;
                custom.HostPageId = entity.hostPageId;
                custom.ItemOrder = entity.itemOrder;
                custom.Required = entity.required;
                custom.LastUpdatedBy = entity.lastUpdatedBy;
                custom.DateTimeUpdated = genSetup.GetApplicationDate().Date;
                foreach (var option in entity.customFieldOption)
                {
                    var options = context.tbl_Custom_Field_Option .SingleOrDefault(c => c.CustomFieldOptionsId == option.customFieldOptionsId);
                    options.OptionsKey = option.optionsKey;
                    options.OptionsValue = option.optionsValue;
                    options.CustomFieldId = option.customFieldId;
                }
                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CustomFieldUpdate,
                    StaffId = entity.lastUpdatedBy,
                    BranchId = (short)entity.userBranchId,
                    Detail = $"updated the following custom field : { entity.labelName} to  {context.tbl_Custom_HostPage.Find(entity.hostPageId).HostPage } form ",
                    IPAddress = entity.userIPAddress,
                    Url = entity.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
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
                    var fieldDetailsUpload = new tbl_Custom_Field_Data_Upload
                    {
                        CustomFieldDataUpload = entity.customFieldDataUpload,
                        CustomFieldsDataId = entity.customFieldsDataId,
                        DateTimeCreated = genSetup.GetApplicationDate().Date,
                        CreatedBy = entity.createdBy
                    };
                    context.tbl_Custom_Field_Data_Upload.Add(fieldDetailsUpload);
                }
                var fieldDetails = new tbl_Custom_Fields_Data
                {
                    CreatedBy = entity.createdBy,

                    OwnerId = entity.ownerId,
                    CustomFieldId = entity.customFieldId,
                    DataDetails = entity.isUpload ? Guid.NewGuid().ToString() : entity.dataDetails,
                    DateTimeCreated = genSetup.GetApplicationDate().Date 
                };
                context.tbl_Custom_Fields_Data.Add(fieldDetails);

                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CustomFieldDatadAdd,
                    StaffId = entity.createdBy,
                    BranchId = (short)entity.userBranchId,
                    Detail = $"Added the following custom field : { entity.labelName} to  {context.tbl_Custom_HostPage.Find(entity.hostPageId).HostPage } form ",
                    IPAddress = entity.userIPAddress,
                    Url = entity.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };

                this.auditTrail.AddAuditTrail(audit);
              
            }
            return await context.SaveChangesAsync() != 0;
        }

        private IEnumerable<CustomFieldsDataViewModel> CustomFieldsData(int hostPageId, int customerId, int companyId)
        {
            var CustomFieldData = context.tbl_Custom_Fields_Data;
            var customs = (from d in context.tbl_Custom_Fields
                           join s in context.tbl_Custom_HostPage on d.HostPageId equals s.HostPageId
                           where s.HostPageId == hostPageId && d.CompanyId == companyId && d.Deleted == false
                           select new CustomFieldsDataViewModel
                           {
                               controlType = d.ControlType,
                               customFieldId = d.CustomFieldId,
                               itemOrder = d.ItemOrder,
                               labelName = d.LabelName,
                               hostPageId = s.HostPageId,
                               parentHostPageId = s.ParentHostPageId,
                               hostPage = s.HostPage,
                               required = d.Required,
                               isUpload = d.IsUpload,
                               controlKey =d.ControlKey,
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
                    var check = CustomFieldData.Where(c => c.CustomFieldId == custom.customFieldId && c.OwnerId == customerId);
                    if (check.Count() > 0)
                    {
                        var data = check.First();
                        custom.customFieldDataUpload = custom.isUpload ? context.tbl_Custom_Field_Data_Upload.FirstOrDefault(d => d.CustomFieldsDataId == data.CustomFieldsDataId).CustomFieldDataUpload : null;
                        custom.customFieldsDataId = data.CustomFieldsDataId;
                        custom.dataDetails = data.DataDetails;
                        custom.isUpload = custom.isUpload;
                        custom.ownerId = data.OwnerId;

                    }
                }
            }
            return customs.ToList();
        }

        public async  Task<bool> DeleteCustomFieldsData(List<CustomFieldsDataViewModel> listEntity, UserInfo user)
        {
            foreach (var entity in listEntity) {
                var field = context.tbl_Custom_Fields_Data.Find(entity.customFieldsDataId);
                field.Deleted = true;
                field.DeletedBy = user.staffId;
                field.DateTimeDeleted = genSetup.GetApplicationDate().Date;

                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CustomFieldDataDelete,
                    StaffId = entity.createdBy,
                    BranchId = (short)entity.userBranchId,
                    Detail = $"Deleted a custome field data for : { entity.labelName} from  {context.tbl_Custom_HostPage.Find(entity.hostPageId).HostPage }  ",
                    IPAddress = entity.userIPAddress,
                    Url = entity.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
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
                    var fieldDetailsUpload = context.tbl_Custom_Field_Data_Upload.Find(entity.customFieldDataUpload);
                    fieldDetailsUpload.CustomFieldDataUpload = entity.customFieldDataUpload;
                    fieldDetailsUpload.CustomFieldsDataId = entity.customFieldsDataId;
                    fieldDetailsUpload.DateTimeCreated = genSetup.GetApplicationDate().Date;
                    fieldDetailsUpload.CreatedBy = entity.createdBy;                      
                }
                var fieldDetails = context.tbl_Custom_Fields_Data.Find(entity.customFieldsDataId);
                fieldDetails.CreatedBy = entity.createdBy;
                fieldDetails.OwnerId = entity.ownerId;
                fieldDetails.CustomFieldId = entity.customFieldId;
                fieldDetails.DataDetails = entity.isUpload ? Guid.NewGuid().ToString() : entity.dataDetails;
                fieldDetails.DateTimeCreated = genSetup.GetApplicationDate().Date;
              

                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CustomFieldDatadAdd,
                    StaffId = entity.createdBy,
                    BranchId = (short)entity.userBranchId,
                    Detail = $"Added the following custom field : { entity.labelName} to  {context.tbl_Custom_HostPage.Find(entity.hostPageId).HostPage } form ",
                    IPAddress = entity.userIPAddress,
                    Url = entity.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
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
            return context.tbl_Custom_HostPage.Select(c => new HostPageViewModel()
            {
                hostPage = c.HostPage,
                hostPageId = c.HostPageId,
                parentHostPageId = c.ParentHostPageId
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
