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

namespace FintrakBanking.Repositories.Setups.Finance
{
    public class ChargeFeeRepository : IChargeFeeRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public ChargeFeeRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
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
                DateTimeCreated = general.GetApplicaionDate()
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
                        DateTimeCreated = general.GetApplicaionDate()
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
                ApplicationDate = general.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
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
            data.DateTimeUpdated = general.GetApplicaionDate();

            // still testing
            //var notRemoved = model.ranges.Select(range => range.chargeRangeId).ToArray();
            //context.tbl_Charge_Range.RemoveRange(
            //    context.tbl_Charge_Range.Where(range => !notRemoved.Contains(range.ChargeRangeId) && range.ChargeFeeId == chargeFeeId)
            //);

            foreach (var range in model.ranges)
            {
                var rangedata = this.context.tbl_Charge_Range.Find(range.chargeRangeId);
                if (rangedata == null)
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
                        DateTimeCreated = general.GetApplicaionDate()
                    });
                }
                else
                {
                    // rangedata.ChargeFeeId = range.chargeFeeId;
                    rangedata.Minimum = range.minimum;
                    rangedata.Maximum = range.maximum;
                    rangedata.Rate = range.rate;
                    rangedata.Amount = range.amount;
                    rangedata.MinimumAndAbove = range.minimumAndAbove;
                    rangedata.MaximumAndBelow = range.maximumAndBelow;
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
                ApplicationDate = general.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
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
            data.DateTimeUpdated = general.GetApplicaionDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChargeFeeDeleted,
                StaffId = user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted ChargeFee '{ data.ChargeFeeName }' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = general.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }
    }
}