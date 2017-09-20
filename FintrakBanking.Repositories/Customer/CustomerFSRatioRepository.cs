using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace FintrakBanking.Repositories.Customer
{
    [Export(typeof(ICustomerFSRatioRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CustomerFSRatioRepository : ICustomerFSRatioRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public CustomerFSRatioRepository ( FinTrakBankingContext _context, 
            IGeneralSetupRepository genSetup, IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        #region tbl_Customer FS Ratio Caption

        public IEnumerable<CustomerFSRatioCaptionViewModel> GetFSRatioCaption(int companyId)
        {
            var data = (from a in context.tbl_Customer_FS_Ratio_Caption
                        where a.CompanyId == companyId && a.Deleted == false
                        orderby a.Position
                        select new CustomerFSRatioCaptionViewModel
                        {
                            ratioCaptionId = a.RatioCaptionId,
                            ratioCaptionName = a.RatioCaption,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            annualised = a.Annualised,
                            position = a.Position,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public List<CustomerFSRatioCaptionViewModel> GetFSRatioCaptionById(short ratioCaptionId)
        {
            var data = (from a in context.tbl_Customer_FS_Ratio_Caption
                        where a.RatioCaptionId == ratioCaptionId // && a.Deleted == false
                        orderby a.Position
                        select new CustomerFSRatioCaptionViewModel
                        {
                            ratioCaptionId = a.RatioCaptionId,
                            ratioCaptionName = a.RatioCaption,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            annualised = a.Annualised,
                            position = a.Position,

                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public bool UpdateFSRatioCaption(short ratioCaptionId, CustomerFSRatioCaptionViewModel model)
        {
            var data = this.context.tbl_Customer_FS_Ratio_Caption.Find(ratioCaptionId);
            if (data == null) return false;

            data.Annualised = model.annualised;
            data.CompanyId = model.companyId;
            data.RatioCaption = model.ratioCaptionName;
            data.Position = model.position;

            data.LastUpdatedBy = (int)model.createdBy;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSRatioCaptionUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated FS Ratio Caption : { data.RatioCaption } with postion: {data.Position}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
              
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool AddFSRatioCaption(CustomerFSRatioCaptionViewModel model)
        {
            var data = new tbl_Customer_FS_Ratio_Caption
            {
                Annualised = model.annualised,
                CompanyId = model.companyId,
                Position = model.position,
                RatioCaption = model.ratioCaptionName,
                CreatedBy = (int)model.createdBy,
                DateTimeCreated = _genSetup.GetApplicationDate()
            };

            context.tbl_Customer_FS_Ratio_Caption.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSRatioCaptionAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added FS Ratio Caption {data.RatioCaption} and postion {data.Position}.",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteFSRatioCaption(short ratioCaptionId, UserInfo user)
        {
            var data = context.tbl_Customer_FS_Ratio_Caption.Find(ratioCaptionId);
            data.Deleted = true;
            data.DeletedBy = (int)user.staffId;
            data.DateTimeDeleted = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSRatioCaptionDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted FS Ratio Caption: { data.RatioCaption }. ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
        #endregion


        #region tbl_Customer FS-Ratio-Detail
        public bool AddFSRatioDetail(CustomerFSRatioDetailViewModel model)
        {
            if (model != null)
            {
                var data = new tbl_Customer_FS_Ratio_Detail
                {
                    RatioCaptionId = model.ratioCaptionId,
                    DivisorTypeId = (short)model.divisorTypeId,
                    FSCaptionId = (int)model.fscaptionId,
                    Multiplier = (double)model.multiplier,
                    ValueTypeId = (short)model.valueTypeId,

                    CreatedBy = (int)model.createdBy,
                    DateTimeCreated = _genSetup.GetApplicationDate()
                };

                context.tbl_Customer_FS_Ratio_Detail.Add(data);

                // Audit Section ---------------------------
                var auditDivisor = context.tbl_Customer_FS_Ratio_DivisorType.FirstOrDefault(x => x.DivisorTypeId == data.DivisorTypeId)?.DivisorTypeName;
                var auditValue = context.tbl_Customer_FS_Ratio_ValueType.FirstOrDefault(x => x.ValueTypeId == data.ValueTypeId)?.ValueTypeName;

                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CustomerFSRatioDetailAdded,
                    StaffId = model.createdBy,
                    BranchId = (short)model.userBranchId,
                    Detail = $"Added FS Ratio Detail {data.tbl_Customer_FS_Ratio_Caption} with divisor type '{auditDivisor}' and value typ '{auditValue }' ",
                    IPAddress = model.userIPAddress,
                    Url = model.applicationUrl,
                    ApplicationDate = _genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };

                this.auditTrail.AddAuditTrail(audit);
            }

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddMultipleFSRatioDetail(List<CustomerFSRatioDetailViewModel> model)
        {
            if (model.Count <= 0)
                return false;

            foreach (CustomerFSRatioDetailViewModel entity in model)
                AddFSRatioDetail(entity);

            return true;
        }

        public IEnumerable<CustomerFSRatioDetailViewModel> GetFSRatioDetail(short ratioCaptionId, short fsCaptionGroupId, int companyId)
        {
            var data = (from a in context.tbl_Customer_FS_Ratio_Detail
                        where a.RatioCaptionId == ratioCaptionId && a.tbl_Customer_FS_Caption.FSCaptionGroupId == fsCaptionGroupId &&
                              a.tbl_Customer_FS_Ratio_Caption.CompanyId == companyId && a.Deleted == false
                                                select new CustomerFSRatioDetailViewModel

                        {
                            ratioDetailId = a.RatioDetailId,
                            ratioCaptionId = a.RatioCaptionId,
                            divisorTypeId = a.DivisorTypeId,
                            valueTypeId = a.ValueTypeId,
                            multiplier = a.Multiplier,
                            fscaptionId = a.FSCaptionId,
                            ratioCaptionName = a.tbl_Customer_FS_Ratio_Caption.RatioCaption,
                            fsCaptionName = a.tbl_Customer_FS_Caption.FSCaptionName,
                            valueTypeName = a.tbl_Customer_FS_Ratio_ValueType.ValueTypeName,
                            divisorTypeName = a.tbl_Customer_FS_Ratio_DivisorType.DivisorTypeName,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data; 
        }

        public CustomerFSRatioDetailViewModel GetFSRatioDetailById(int ratioDetailId)
        {
            var data = from a in context.tbl_Customer_FS_Ratio_Detail
                       where a.RatioDetailId == ratioDetailId && a.Deleted == false
                       select new CustomerFSRatioDetailViewModel
                       {
                           ratioDetailId = a.RatioDetailId,
                           ratioCaptionId = a.RatioCaptionId,
                           divisorTypeId = a.DivisorTypeId,
                           valueTypeId = a.ValueTypeId,
                           multiplier = a.Multiplier,
                           fscaptionId = a.FSCaptionId,
                           ratioCaptionName = a.tbl_Customer_FS_Ratio_Caption.RatioCaption,
                           fsCaptionName = a.tbl_Customer_FS_Caption.FSCaptionName,
                           valueTypeName = a.tbl_Customer_FS_Ratio_ValueType.ValueTypeName,
                           divisorTypeName = a.tbl_Customer_FS_Ratio_DivisorType.DivisorTypeName,
                           dateTimeCreated = a.DateTimeCreated,
                           createdBy = a.CreatedBy
                       };

            return data.SingleOrDefault();
        }

        public bool UpdateFSRatioDetail(int ratioDetailId, CustomerFSRatioDetailViewModel model)
        {
            var data = this.context.tbl_Customer_FS_Ratio_Detail.Find(ratioDetailId);
            if (data == null) return false;

            data.RatioCaptionId = model.ratioCaptionId;
            data.Multiplier = (double)model.multiplier;
            data.ValueTypeId = (short)model.valueTypeId;
            data.DivisorTypeId = (short)model.divisorTypeId;
            data.FSCaptionId = (int)model.fscaptionId;
            data.LastUpdatedBy = model.createdBy;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit_divisor = (context.tbl_Customer_FS_Ratio_DivisorType.FirstOrDefault(x => x.DivisorTypeId == data.DivisorTypeId)).DivisorTypeName;
            var audit_value = (context.tbl_Customer_FS_Ratio_ValueType.FirstOrDefault(x => x.ValueTypeId == data.ValueTypeId)).ValueTypeName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSRatioDetailUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated FS Ratio Detail {data.tbl_Customer_FS_Ratio_Caption} with divisor type '{audit_divisor}' and value typ '{audit_value }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;

        }

        public bool DeleteFSRatioDetail(int ratioDetailId, UserInfo user)
        {
            var data = context.tbl_Customer_FS_Ratio_Detail.Find(ratioDetailId);
            data.Deleted = true;
            data.DeletedBy = (int)user.staffId;
            data.DateTimeDeleted = _genSetup.GetApplicationDate();


            // Audit Section ---------------------------
            var audit_divisor = (context.tbl_Customer_FS_Ratio_DivisorType.FirstOrDefault(x => x.DivisorTypeId == data.DivisorTypeId)).DivisorTypeName;
            var audit_value = (context.tbl_Customer_FS_Ratio_ValueType.FirstOrDefault(x => x.ValueTypeId == data.ValueTypeId)).ValueTypeName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSRatioDetailDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted FS Ratio Detail {data.tbl_Customer_FS_Ratio_Caption} with divisor type '{audit_divisor}' and value type '{audit_value }' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;
        }

        public bool DeleteMultipleFSRatioDetail(List<int> ratioDetailId, UserInfo user)
        {
            if (ratioDetailId.Count <= 0)
                return false;

            foreach (int fsdetailId in ratioDetailId)
                DeleteFSRatioDetail(fsdetailId, user);

            return true;
        }
        #endregion

        #region tbl_Customer FS Ratio Divisor Type
        public IEnumerable<CustomerFSRatioDivisorTypeViewModel> GetAllDivisorType()
        {
            var data = (from a in context.tbl_Customer_FS_Ratio_DivisorType
                        where a.Deleted == false
                        select new CustomerFSRatioDivisorTypeViewModel
                        {
                            divisorTypeId = a.DivisorTypeId,
                            divisorTypeName = a.DivisorTypeName,
                            //createdBy = a.CreatedBy.Value,
                            dateTimeCreated = a.DateTimeCreated
                        }).ToList();
            return data;
        }
        #endregion


        #region tbl_Customer FS Ration Value Type
        public IEnumerable<CustomerFSRatioValueTypeViewModel> GetAllValueType()
        {
            var data = (from a in context.tbl_Customer_FS_Ratio_ValueType
                        where a.Deleted == false
                        select new CustomerFSRatioValueTypeViewModel
                        {
                            valueTypeId = a.ValueTypeId,
                            valueTypeName = a.ValueTypeName,
                            dateTimeCreated = a.DateTimeCreated,
                            //createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }
        #endregion
    }
}
