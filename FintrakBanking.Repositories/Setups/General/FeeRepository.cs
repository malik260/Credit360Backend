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
    [Export(typeof(IFeeRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FeeRepository : IFeeRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;

        public FeeRepository(FinTrakBankingContext _context,
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

        #region tbl_Product Fee

  


        public int AddFee(FeeViewModel fee)
        {
            var data = new tbl_Fee()
            {
                FeeName = fee.feeName,
                AccountCategoryId = fee.accountCategoryId,
                FeeTypeId = fee.feeTypeId,
                FeeIntervalId = fee.feeIntervalId,
                ProductTypeId = fee.productTypeId,
                FeeTargetId = fee.feeTargetId,
                IsIntegralFee = fee.isIntegralFee,
                GLAccountId = fee.glAccountId,
                FeeAmortisationTypeId = fee.feeAmortisationTypeId,
                IncludeCutOffDay = fee.includeCutOffDay,
                CutOffDay = fee.cutOffDay,
                CompanyId = fee.companyId,
                FeeDate = DateTime.Now,

                CreatedBy = fee.createdBy,
                DateTimeCreated = DateTime.Now,
            };           

            this.context.tbl_Fee.Add(data);

            var status = this.SaveAll();

            if (status)
            {
                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.CollateralCategoryAdded,
                    StaffId = fee.createdBy,
                    BranchId = (short)fee.userBranchId,
                    Detail = $"Added fee: { fee.feeName } of type {fee.feeTypeName} ",
                    IPAddress = fee.userIPAddress,
                    Url = fee.applicationUrl,
                    ApplicationDate = genSetup.GetApplicaionDate(),
                    SystemDateTime = DateTime.Now
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------

                return data.FeeId;
            }
            
            else
                return -1;
        }

        public IEnumerable<FeeViewModel> GetAllFee()
        {
            return (from data in context.tbl_Fee
                        //where account.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new FeeViewModel()
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
                        isIntegralFee = data.IsIntegralFee,
                        feeIntervalName = data.tbl_Fee_Interval.FeeIntervalName,
                        productTypeId = data.ProductTypeId,
                        productTypeName = data.tbl_Product_Type.ProductTypeName,
                        feeTargetId = data.FeeTargetId,
                        feeTargetName = data.tbl_Fee_Target.FeeTargetName,
                        glAccountId = data.GLAccountId,
                        glAccountCode = data.tbl_Chart_Of_Account.AccountCode,
                        glAccountName = data.tbl_Chart_Of_Account.AccountName,
                        includeCutOffDay = data.IncludeCutOffDay,
                        feeAmortisationTypeId = data.FeeAmortisationTypeId,

                        cutOffDay = data.CutOffDay,
                        companyId = data.CompanyId,
                        feeDate = data.FeeDate,
                        createdBy = data.CreatedBy,
                        
                        dateTimeCreated = data.DateTimeCreated,
                        dateTimeUpdated = data.DateTimeUpdated,
                        deleted = data.Deleted,
                        deletedBy = data.DeletedBy,
                        dateTimeDeleted = data.DateTimeDeleted
                    });
        }

        public FeeViewModel GetFeeViewModel(int feeId)
        {
            return (from data in context.tbl_Fee
                    where data.FeeId == feeId //orderby account.AccountCode ascending, account.AccountName ascending
                    select new FeeViewModel()
                    {
                        feeId = data.FeeId,
                        feeName = data.FeeName,
                        accountCategoryId = data.AccountCategoryId,
                        feeTypeId = data.FeeTypeId,
                        feeIntervalId = data.FeeIntervalId,
                        productTypeId = data.ProductTypeId,
                        feeTargetId = data.FeeTargetId,
                        glAccountId = data.GLAccountId,
                        feeAmortisationTypeId = data.FeeAmortisationTypeId,
                        isIntegralFee = data.IsIntegralFee,
                        includeCutOffDay = data.IncludeCutOffDay,
                        cutOffDay = data.CutOffDay,
                        companyId = data.CompanyId,
                        feeDate = data.FeeDate,
                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated,
                        dateTimeUpdated = data.DateTimeUpdated,
                        deleted = data.Deleted,
                        deletedBy = data.DeletedBy,
                        dateTimeDeleted = data.DateTimeDeleted,
                    }).FirstOrDefault();
        }

        public bool UpdateFee(int feeId, FeeViewModel fee)
        {
            var feeModel = this.context.tbl_Fee.Find(feeId);

            if (feeModel == null)
                return false;

            feeModel.FeeName = fee.feeName;
            feeModel.AccountCategoryId = fee.accountCategoryId;
            feeModel.FeeTypeId = fee.feeTypeId;
            feeModel.FeeIntervalId = fee.feeIntervalId;
            feeModel.ProductTypeId = fee.productTypeId;
            feeModel.FeeTargetId = fee.feeTargetId;
            feeModel.IsIntegralFee = fee.isIntegralFee;
            feeModel.GLAccountId = fee.glAccountId;
            feeModel.FeeAmortisationTypeId = fee.feeAmortisationTypeId;
            feeModel.IncludeCutOffDay = fee.includeCutOffDay;
            feeModel.CutOffDay = fee.cutOffDay;
            feeModel.CompanyId = fee.companyId;
            feeModel.FeeDate = fee.feeDate;            

            feeModel.LastUpdatedBy = fee.lastUpdatedBy;
            feeModel.DateTimeUpdated = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralCategoryAdded,
                StaffId = fee.createdBy,
                BranchId = (short)fee.userBranchId,
                Detail = $"Udated fee: { fee.feeName } of type {fee.feeTypeName} ",
                IPAddress = fee.userIPAddress,
                Url = fee.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        #endregion tbl_Product Fee

        #region Fee Related Lookups

        public IEnumerable<LookupViewModel> GetFeeAccountCategory()
        {
            return (from data in context.tbl_Account_Category
                    where data.AccountCategoryId == (short)AccountCategoryEnum.Income || data.AccountCategoryId == (short)AccountCategoryEnum.Expense
                    //orderby data.FinType, data.Position
                    select new LookupViewModel()
                    {
                        lookupId = data.AccountCategoryId,
                        lookupName = data.AccountCategoryName
                    });
        }

        public IEnumerable<LookupViewModel> GetFeeType()
        {
            return (from data in context.tbl_Fee_Type
                        //orderby data.FinType, data.Position
                    select new LookupViewModel()
                    {
                        lookupId = data.FeeTypeId,
                        lookupName = data.FeeTypeName
                    });
        }

        public IEnumerable<LookupViewModel> GetFeeInterval()
        {
            return (from data in context.tbl_Fee_Interval
                        //orderby data.FinType, data.Position
                    select new LookupViewModel()
                    {
                        lookupId = data.FeeIntervalId,
                        lookupName = data.FeeIntervalName
                    });
        }

        public IEnumerable<LookupViewModel> GetFeeTarget()
        {
            return (from data in context.tbl_Fee_Target
                    select new LookupViewModel()
                    {
                        lookupId = data.FeeTargetId,
                        lookupName = data.FeeTargetName
                    });
        }

        #endregion Fee Related Lookups
    }
}