using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.ViewModels.Business;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.Repositories.Setups.Finance
{
    public class ChargeFeeRepository : IChargeFeeRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository auditTrail;
        private IWorkFlowRepository workFlow;

        public ChargeFeeRepository(FinTrakBankingContext context, IGeneralSetupRepository general, 
                                    IAuditTrailRepository _auditTrail, IWorkFlowRepository _workFlow)
        {
            this.context = context;
            this.general = general;
            this.auditTrail = _auditTrail;
            this.workFlow = _workFlow;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)Operations.UserCreation;

            var response = workFlow.GoForApproval(entity);

            if (response.Result.Item1)
            {
                return ApproveChargeFee(entity.targetId, response.Result.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }

        }

        private bool ApproveChargeFee(int userid, short approvalStatusId, UserInfo user)
        {
            return true;
        }

        public bool AddTempChargeFee(ChargeFeeViewModel chargeFeemodel)
        {
            bool output = false;
            var existStingTempChargeFee = context.tbl_Temp_Charge_Fee.Where(x => x.ChargeFeeName.ToLower() == chargeFeemodel.chargeName.ToLower()
                                                                  && x.IsCurrent == true
                                                                  && x.CompanyId == chargeFeemodel.companyId
                                                                  && x.ApprovalStatusId == (short)ApprovalStatusEnum.Pending);

            if (existStingTempChargeFee.Any())
            {
                throw new Exception("Charge Fee Information already exist and is undergoing approval");
            }

            var chargeFee = new tbl_Temp_Charge_Fee()
            {
                ChargeFeeName = chargeFeemodel.chargeName,
                AccountCategoryId = chargeFeemodel.accountCategoryId,
                FeeIntervalId = chargeFeemodel.frequencyTypeId,
                ProductTypeId = chargeFeemodel.productId,
                FeeTargetId = chargeFeemodel.targetId,
                GLAccountId = chargeFeemodel.ledgerAccountId,
                FeeAmortisationTypeId = chargeFeemodel.amortisationTypeId,
                IsIntegralFee = chargeFeemodel.isIntegral,
                IncludeCutOffDay = chargeFeemodel.includeCutOffDay,
                CutOffDay = chargeFeemodel.cutOffDay,
                OperationId = chargeFeemodel.operationId,
                Amount = chargeFeemodel.amount,
                Rate = chargeFeemodel.rate,
                ValueSource = chargeFeemodel.valueSource,
                Recurring = chargeFeemodel.recurring,
                PrimaryTaxId = chargeFeemodel.primaryTaxId,
                SecondaryTaxId = chargeFeemodel.secondaryTaxId,
                CompanyId = chargeFeemodel.companyId,
                CreatedBy = (int)chargeFeemodel.createdBy,
                DateTimeCreated = general.GetApplicationDate(),
                //ApprovalStatusId = (short)ApprovalStatusEnum.Pending,
                //IsCurrent = true

            };
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CreateStaffInitiated,
                StaffId = chargeFeemodel.createdBy,
                BranchId = (short)chargeFeemodel.userBranchId,
                Detail ="", // $"Initiated Staff Creation for '{staffModel.StaffFullName}' with code'{staffModel.StaffCode}'",
                IPAddress = chargeFeemodel.userIPAddress,
                Url = chargeFeemodel.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            if (workFlow.CheckRouteForOperation((int)Operations.FeeCreation, chargeFeemodel.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        auditTrail.AddAuditTrail(audit);
                        this.context.tbl_Temp_Charge_Fee.Add(chargeFee);
                        output = context.SaveChanges() != 0;

                        var entity = new ApprovalViewModel
                        {
                            staffId = chargeFeemodel.createdBy,
                            companyId = chargeFeemodel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = chargeFee.ChargeFeeId,
                            operationId = (int)Operations.FeeCreation,
                            BranchId = chargeFeemodel.userBranchId
                        };
                        var response = workFlow.LogForApproval(entity);
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                    }
                }
            }
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }
            return output;

        }

        public bool AddChargeFee(ChargeFeeViewModel model)
        {
            var data = new tbl_Charge_Fee
            {
                ChargeFeeName = model.chargeName,
                AccountCategoryId = model.accountCategoryId,
                FeeIntervalId = model.frequencyTypeId,
                ProductTypeId = model.productId,
                FeeTargetId = model.targetId,
                GLAccountId = model.ledgerAccountId,
                FeeAmortisationTypeId = model.amortisationTypeId,
                IsIntegralFee = model.isIntegral,
                IncludeCutOffDay = model.includeCutOffDay,
                CutOffDay = model.cutOffDay,
                OperationId = model.operationId,
                Amount = model.amount,
                Rate = model.rate,
                ValueSource = model.valueSource,
                Recurring = model.recurring,
                PrimaryTaxId = model.primaryTaxId,
                SecondaryTaxId = model.secondaryTaxId,
                CompanyId = model.companyId,
                CreatedBy = (int)model.createdBy,
                DateTimeCreated = general.GetApplicationDate()
            };
            
            context.tbl_Charge_Fee.Add(data);
            context.SaveChanges();

            if (data.ChargeFeeId != 0)
            {
                foreach (var range in model.ranges)
                {
                    context.tbl_Charge_Range.Add(new tbl_Charge_Range
                    {
                        ChargeFeeId = data.ChargeFeeId,
                        Minimum = range.minimum,
                        Maximum = range.maximum,
                        Rate = range.rate,
                        Amount = range.amount,
                        MinimumAndAbove = range.minimumAndAbove,
                        MaximumAndBelow = range.maximumAndBelow,
                        CreatedBy = (int)model.createdBy,
                        DateTimeCreated = general.GetApplicationDate()
                    });
                }
            }

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChargeFeeAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added ChargeFee '{ data.ChargeFeeName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateChargeFee(ChargeFeeViewModel model, int chargeFeeId)
        {
            var data = this.context.tbl_Charge_Fee.Find(chargeFeeId);
            if (data == null)
            {
                return false;
            }

            data.ChargeFeeName = model.chargeName;
            data.AccountCategoryId = model.accountCategoryId;
            data.FeeIntervalId = model.frequencyTypeId;
            data.ProductTypeId = model.productId;
            data.FeeTargetId = model.targetId;
            data.GLAccountId = model.ledgerAccountId;
            data.FeeAmortisationTypeId = model.amortisationTypeId;
            data.IsIntegralFee = model.isIntegral;
            data.IncludeCutOffDay = model.includeCutOffDay;
            data.CutOffDay = model.cutOffDay;
            data.OperationId = model.operationId;
            data.Amount = model.amount;
            data.Rate = model.rate;
            data.ValueSource = model.valueSource;
            data.Recurring = model.recurring;
            data.PrimaryTaxId = model.primaryTaxId;
            data.SecondaryTaxId = model.secondaryTaxId;
            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = general.GetApplicationDate();

            var notRemoved = model.ranges.Select(range => range.chargeRangeId).ToArray();
            context.tbl_Charge_Range.RemoveRange(
                context.tbl_Charge_Range.Where(range => !notRemoved.Contains(range.ChargeRangeId) && range.ChargeFeeId == chargeFeeId)
            );

            var count = model.ranges.Count();

            foreach (var range in model.ranges)
            {
                if (range.chargeRangeId <= 0)
                {
                    context.tbl_Charge_Range.Add(new tbl_Charge_Range
                    {
                        ChargeFeeId = chargeFeeId,
                        Minimum = range.minimum,
                        Maximum = range.maximum,
                        Rate = range.rate,
                        Amount = range.amount,
                        MinimumAndAbove = range.minimumAndAbove,
                        MaximumAndBelow = range.maximumAndBelow,
                        CreatedBy = (int)model.createdBy,
                        DateTimeCreated = general.GetApplicationDate()
                    });
                }
            }

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChargeFeeUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated ChargeFee '{ data.ChargeFeeName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ChargeFeeViewModel> GetAllChargeFee()
        {
            return this.context.tbl_Charge_Fee.Where(x => x.Deleted == false).Select(x => new ChargeFeeViewModel
            {
                chargeFeeId = x.ChargeFeeId,
                chargeName = x.ChargeFeeName,
                accountCategoryId = x.AccountCategoryId,
                frequencyTypeId = x.FeeIntervalId,
                productId = x.ProductTypeId,
                targetId = x.FeeTargetId,
                ledgerAccountId = x.GLAccountId,
                amortisationTypeId = x.FeeAmortisationTypeId,
                isIntegral = x.IsIntegralFee,
                includeCutOffDay = x.IncludeCutOffDay,
                cutOffDay = x.CutOffDay,
                operationId = x.OperationId,
                amount = x.Amount,
                rate = x.Rate,
                valueSource = x.ValueSource,
                recurring = (bool)x.Recurring,
                primaryTaxId = x.PrimaryTaxId,
                secondaryTaxId = x.SecondaryTaxId,
                ranges = context.tbl_Charge_Range.Where(r => r.ChargeFeeId == x.ChargeFeeId)
                    .Select(r => new ChargeRangeViewModel {
                        chargeRangeId = r.ChargeRangeId,
                        minimum = r.Minimum,
                        maximum = r.Maximum,
                        amount = r.Amount,
                        rate = r.Rate,
                        minimumAndAbove = r.MinimumAndAbove,
                        maximumAndBelow = r.MaximumAndBelow,
                        chargeFeeId = r.ChargeFeeId
                    }).ToList(),
            });
        }

        public ChargeFeeViewModel GetChargeFee(int chargeFeeId)
        {
            var data = this.context.tbl_Charge_Fee.Find(chargeFeeId);

            if (data == null)
            {
                return null;
            }

            return new ChargeFeeViewModel
            {
                chargeFeeId = data.ChargeFeeId,
                chargeName = data.ChargeFeeName,
                accountCategoryId = data.AccountCategoryId,
                frequencyTypeId = data.FeeIntervalId,
                productId = data.ProductTypeId,
                targetId = data.FeeTargetId,
                ledgerAccountId = data.GLAccountId,
                amortisationTypeId = data.FeeAmortisationTypeId,
                isIntegral = data.IsIntegralFee,
                includeCutOffDay = data.IncludeCutOffDay,
                cutOffDay = data.CutOffDay,
                operationId = data.OperationId,
                amount = data.Amount,
                rate = data.Rate,
                valueSource = data.ValueSource,
                recurring = (bool)data.Recurring,
                primaryTaxId = data.PrimaryTaxId,
                secondaryTaxId = data.SecondaryTaxId,
                ranges = context.tbl_Charge_Range.Where(r => r.ChargeFeeId == data.ChargeFeeId)
                    .Select(r => new ChargeRangeViewModel
                    {
                        chargeRangeId = r.ChargeRangeId,
                        minimum = r.Minimum,
                        maximum = r.Maximum,
                        amount = r.Amount,
                        rate = r.Rate,
                        minimumAndAbove = r.MinimumAndAbove,
                        maximumAndBelow = r.MaximumAndBelow,
                        chargeFeeId = r.ChargeFeeId
                    }).ToList(),
            };
        }

        public IEnumerable<ChargeFeeViewModel> GetAllChargeFeeByCompanyId(int companyId)
        {
            return this.GetAllChargeFee().Where(x => x.companyId == companyId);
        }

        public bool DeleteChargeFee(int chargeFeeId, UserInfo user)
        {
            var data = this.context.tbl_Charge_Fee.Find(chargeFeeId);
            if (data == null)
            {
                return false;
            }

            data.Deleted = true;
            data.DateTimeUpdated = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChargeFeeDeleted,
                StaffId = user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted ChargeFee '{ data.ChargeFeeName }' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

    }
}