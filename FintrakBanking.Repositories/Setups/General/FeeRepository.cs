using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Business;
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
        private IWorkFlowRepository workFlow;


        public FeeRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                        IWorkFlowRepository _workFlow)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            workFlow = _workFlow;
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
                    AuditTypeId = (short)AuditTypeEnum.FeeAdded,
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

        public bool AddTempFee(FeeViewModel feeModel)
        {
            bool output = false;

            var tempFee = new tbl_Temp_Fee()
            {
                FeeName = feeModel.feeName,
                AccountCategoryId = feeModel.accountCategoryId,
                FeeTypeId = feeModel.feeTypeId,
                FeeIntervalId = feeModel.feeIntervalId,
                ProductTypeId = feeModel.productTypeId,
                FeeTargetId = feeModel.feeTargetId,
                IsIntegralFee = feeModel.isIntegralFee,
                GLAccountId = feeModel.glAccountId,
                FeeAmortisationTypeId = feeModel.feeAmortisationTypeId,
                IncludeCutOffDay = feeModel.includeCutOffDay,
                CutOffDay = feeModel.cutOffDay,
                CompanyId = feeModel.companyId,
                FeeDate = DateTime.Now,

                CreatedBy = feeModel.createdBy,
                DateTimeCreated = DateTime.Now,
            };

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.FeeAdded,
                StaffId = feeModel.createdBy,
                BranchId = (short)feeModel.userBranchId,
                Detail = $"Added fee: { feeModel.feeName } of type {feeModel.feeTypeName} ",
                IPAddress = feeModel.userIPAddress,
                Url = feeModel.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------


            if (workFlow.CheckRouteForOperation((int)Operations.FeeCreation, feeModel.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        this.auditTrail.AddAuditTrail(audit);
                        this.context.tbl_Temp_Fee.Add(tempFee);

                        output = this.SaveAll();

                        var entity = new ApprovalViewModel
                        {
                            staffId = feeModel.createdBy,
                            companyId = feeModel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = feeModel.feeId,
                            operationId = (int)Operations.FeeCreation,
                            BranchId = feeModel.userBranchId
                        };
                        var response = workFlow.LogForApproval(entity);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }

            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }

            return output;

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
                        feeTypeName = string.Empty,//data.tbl_Fee_Type.FeeTypeName,
                        feeTypeByAmountRequired = false,//data.tbl_Fee_Type.ByAmountRequired,
                        byAmountRequired = false,//data.tbl_Fee_Type.ByAmountRequired,
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

        public bool UpdateFeeForApproval(int feeId, FeeViewModel feeModel)
        {
            if (feeModel == null)
                return false;

            var existStingTempFee = context.tbl_Temp_Fee.Where(x => x.AccountCategoryId ==
            feeModel.accountCategoryId && x.IsCurrent == true &&
            x.ApprovalStatusId == (int)ApprovalStatusEnum.Approved);

            if (existStingTempFee.Any())
            {
                foreach (var item in existStingTempFee)
                {
                    item.IsCurrent = false;
                    item.DateTimeUpdated = DateTime.Now;
                }
            }

            var targetFee = this.context.tbl_Fee.Find(feeId);

            var unApprovedFeeEdit = context.tbl_Temp_Fee.Where(x => x.IsCurrent == true
            && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending);

            tbl_Temp_Fee tempFee;

            if (unApprovedFeeEdit.Any())
            {
                throw new Exception("Fee is already undergoing approval");
            }
            else
            {
                tempFee = new tbl_Temp_Fee()
                {
                    FeeName = feeModel.feeName,
                    AccountCategoryId = targetFee.AccountCategoryId,
                    FeeTypeId = feeModel.feeTypeId,
                    FeeIntervalId = feeModel.feeIntervalId,
                    ProductTypeId = feeModel.productTypeId,
                    FeeTargetId = feeModel.feeTargetId,
                    IsIntegralFee = feeModel.isIntegralFee,
                    GLAccountId = feeModel.glAccountId,
                    FeeAmortisationTypeId = feeModel.feeAmortisationTypeId,
                    IncludeCutOffDay = feeModel.includeCutOffDay,
                    CutOffDay = feeModel.cutOffDay,
                    CompanyId = feeModel.companyId,
                    FeeDate = DateTime.Now,

                    CreatedBy = feeModel.createdBy,
                    DateTimeCreated = DateTime.Now,
                };

                context.tbl_Temp_Fee.Add(tempFee);
            }

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.FeeUpdated,
                StaffId = feeModel.createdBy,
                BranchId = (short)feeModel.userBranchId,
                Detail = $"Updated Fee: { feeModel.feeName } with fee account category '{feeModel.accountCategoryName}'",
                IPAddress = feeModel.userIPAddress,
                Url = feeModel.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now,
                TargetId = feeId
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section ------------------------------- 

            var output = this.SaveAll();

            var approvalEntity = new ApprovalViewModel
            {
                staffId = feeModel.createdBy,
                companyId = feeModel.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = tempFee.FeeId,
                operationId = (int)Operations.FeeCreation,
                BranchId = feeModel.userBranchId
            };
            var response = workFlow.LogForApproval(approvalEntity);

            return output;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)Operations.FeeCreation;

            var response = workFlow.GoForApproval(entity);

            if (response.Result.Item1)
            {
                return ApproveFee(entity.targetId, response.Result.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }

        }

        private bool ApproveFee(int feeId, short approvalStatusId, UserInfo user)
        {
            var feeModel = context.tbl_Temp_Fee.Find(feeId);
            var feeToUpdate = context.tbl_Fee.Where(x => x.AccountCategoryId == feeModel.AccountCategoryId);
            var existingFee = feeToUpdate.FirstOrDefault();

            //Update existing fee with tempFee record
            if (feeToUpdate.Any())
            {
                existingFee.FeeName = feeModel.FeeName;
                existingFee.AccountCategoryId = feeModel.AccountCategoryId;
                existingFee.FeeTypeId = feeModel.FeeTypeId;
                existingFee.FeeIntervalId = feeModel.FeeIntervalId;
                existingFee.ProductTypeId = feeModel.ProductTypeId;
                existingFee.FeeTargetId = feeModel.FeeTargetId;
                existingFee.IsIntegralFee = feeModel.IsIntegralFee;
                existingFee.GLAccountId = feeModel.GLAccountId;
                existingFee.FeeAmortisationTypeId = feeModel.FeeAmortisationTypeId;
                existingFee.IncludeCutOffDay = feeModel.IncludeCutOffDay;
                existingFee.CutOffDay = feeModel.CutOffDay;
                existingFee.CompanyId = feeModel.CompanyId;
                existingFee.FeeDate = feeModel.FeeDate;

                existingFee.CreatedBy = feeModel.CreatedBy;
                existingFee.DateTimeUpdated = DateTime.Now;
            }
            else //Insert a newfee record into the real fee table
            {
                var fee = new tbl_Fee()
                {
                    FeeName = feeModel.FeeName,
                    AccountCategoryId = feeModel.AccountCategoryId,
                    FeeTypeId = feeModel.FeeTypeId,
                    FeeIntervalId = feeModel.FeeIntervalId,
                    ProductTypeId = feeModel.ProductTypeId,
                    FeeTargetId = feeModel.FeeTargetId,
                    IsIntegralFee = feeModel.IsIntegralFee,
                    GLAccountId = feeModel.GLAccountId,
                    FeeAmortisationTypeId = feeModel.FeeAmortisationTypeId,
                    IncludeCutOffDay = feeModel.IncludeCutOffDay,
                    CutOffDay = feeModel.CutOffDay,
                    CompanyId = feeModel.CompanyId,
                    FeeDate = DateTime.Now,
                    DateTimeCreated = DateTime.Now
                };
                context.tbl_Fee.Add(fee);

            }

            feeModel.IsCurrent = false;
            feeModel.ApprovalStatusId = approvalStatusId;
            feeModel.DateTimeUpdated = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.FeeApproved,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Fee '{feeModel.FeeName}' with fee account category '{feeModel.tbl_Account_Category.AccountCategoryName}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

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
            //return (from data in context.tbl_Fee_Type
            //            //orderby data.FinType, data.Position
            //        select new LookupViewModel()
            //        {
            //            lookupId = data.FeeTypeId,
            //            lookupName = data.FeeTypeName
            //        });
            return null;
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