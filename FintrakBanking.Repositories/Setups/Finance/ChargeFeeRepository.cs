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
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.WorkFlow;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.Finance
{
    public class ChargeFeeRepository : IChargeFeeRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository auditTrail;

        public ChargeFeeRepository(
            FinTrakBankingContext context, 
            IGeneralSetupRepository general, 
            IAuditTrailRepository _auditTrail
            )
        {
            this.context = context;
            this.general = general;
            this.auditTrail = _auditTrail;
        }

        public async Task<bool> GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.UserCreation;
            /*
            var response = await workFlow.GoForApproval(entity);

            if (response.Item1)
            {
                return ApproveChargeFee(entity.targetId, response.Item2.approvalStatusId, entity);
            }
            else
            {*/
                return false;
            //}

        }

        private bool ApproveChargeFee(int userid, short approvalStatusId, UserInfo user)
        {
            return true;
        }

        public bool AddTempChargeFee(ChargeFeeViewModel chargeFeemodel)
        {
            bool output = false;
            var existStingTempChargeFee = context.TBL_TEMP_CHARGE_FEE.Where(x => x.CHARGEFEENAME.ToLower() == chargeFeemodel.chargeName.ToLower()
                                                                  && x.ISCURRENT == true
                                                                  && x.COMPANYID == chargeFeemodel.companyId
                                                                  && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending);

            if (existStingTempChargeFee.Any())
            {
                throw new Exception("Charge Fee Information already exist and is undergoing approval");
            }

            var chargeFee = new TBL_TEMP_CHARGE_FEE()
            {
                CHARGEFEENAME = chargeFeemodel.chargeName,
                ACCOUNTCATEGORYID = chargeFeemodel.accountCategoryId,
                FEEINTERVALID = chargeFeemodel.frequencyTypeId,
                PRODUCTTYPEID = chargeFeemodel.productId,
                FEETARGETID = chargeFeemodel.targetId,
                GLACCOUNTID = chargeFeemodel.ledgerAccountId,
                FEEAMORTISATIONTYPEID = chargeFeemodel.amortisationTypeId,
                ISINTEGRALFEE = chargeFeemodel.isIntegral,
                INCLUDECUTOFFDAY = chargeFeemodel.includeCutOffDay,
                CUTOFFDAY = chargeFeemodel.cutOffDay,
                OPERATIONID = chargeFeemodel.operationId,
                AMOUNT = chargeFeemodel.amount,
                RATE = chargeFeemodel.rate,

                //ValueSource = chargeFeemodel.feeTypeId,
                FEETYPEID = chargeFeemodel.valueSource,

                RECURRING = chargeFeemodel.recurring,
                PRIMARYTAXID = chargeFeemodel.primaryTaxId,
                SECONDARYTAXID = chargeFeemodel.secondaryTaxId,
                COMPANYID = chargeFeemodel.companyId,
                CREATEDBY = (int)chargeFeemodel.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                //ApprovalStatusId = (short)ApprovalStatusEnum.Pending,
                //IsCurrent = true

            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreateStaffInitiated,
                STAFFID = chargeFeemodel.createdBy,
                BRANCHID = (short)chargeFeemodel.userBranchId,
                DETAIL ="", // $"Initiated Staff Creation for '{staffModel.StaffFullName}' with code'{staffModel.StaffCode}'",
                IPADDRESS = chargeFeemodel.userIPAddress,
                URL = chargeFeemodel.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
/*
            if (workFlow.CheckRouteForOperation((int)OperationsEnum.FeeCreation, chargeFeemodel.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        auditTrail.AddAuditTrail(audit);
                        this.context.TBL_TEMP_CHARGE_FEE.Add(chargeFee);
                        output = context.SaveChanges() != 0;

                        var entity = new ApprovalViewModel
                        {
                            staffId = chargeFeemodel.createdBy,
                            companyId = chargeFeemodel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = (int)chargeFee.CHARGEFEEID,
                            operationId = (int)OperationsEnum.FeeCreation,
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
            }*/
            return output;

        }

        public bool AddChargeFee(ChargeFeeViewModel model)
        {
            var data = new TBL_CHARGE_FEE
            {
                CHARGEFEENAME = model.chargeName,
                ACCOUNTCATEGORYID = model.accountCategoryId,
                FEEINTERVALID = model.frequencyTypeId,
                PRODUCTTYPEID = model.productId,
                FEETARGETID = model.targetId,
                GLACCOUNTID = model.ledgerAccountId,
                FEEAMORTISATIONTYPEID = model.amortisationTypeId,
                ISINTEGRALFEE = model.isIntegral,
                INCLUDECUTOFFDAY = model.includeCutOffDay,
                CUTOFFDAY = model.cutOffDay,
                OPERATIONID = model.operationId,
                AMOUNT = model.amount,
                RATE = model.rate,
                FEETYPEID = model.valueSource,
                RECURRING = model.recurring,
                PRIMARYTAXID = model.primaryTaxId,
                SECONDARYTAXID = model.secondaryTaxId,
                COMPANYID = model.companyId,
                CREATEDBY = (int)model.createdBy,
                DATETIMECREATED = general.GetApplicationDate()
            };
            
            context.TBL_CHARGE_FEE.Add(data);
            context.SaveChanges();

            if (data.CHARGEFEEID != 0)
            {
                foreach (var range in model.ranges)
                {
                    context.TBL_CHARGE_RANGE.Add(new TBL_CHARGE_RANGE
                    {
                        CHARGEFEEID = data.CHARGEFEEID,
                        MINIMUM = range.minimum,
                        MAXIMUM = range.maximum,
                        RATE = range.rate,
                        AMOUNT = range.amount,
                        MINIMUMANDABOVE = range.minimumAndAbove,
                        MAXIMUMANDBELOW = range.maximumAndBelow,
                        CREATEDBY = (int)model.createdBy,
                        DATETIMECREATED = general.GetApplicationDate()
                    });
                }
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChargeFeeAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added ChargeFee '{ data.CHARGEFEENAME }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateChargeFee(ChargeFeeViewModel model, int chargeFeeId)
        {
            var data = this.context.TBL_CHARGE_FEE.Find(chargeFeeId);
            if (data == null)
            {
                return false;
            }

            data.CHARGEFEENAME = model.chargeName;
            data.ACCOUNTCATEGORYID = model.accountCategoryId;
            data.FEEINTERVALID = model.frequencyTypeId;
            data.PRODUCTTYPEID = model.productId;
            data.FEETARGETID = model.targetId;
            data.GLACCOUNTID = model.ledgerAccountId;
            data.FEEAMORTISATIONTYPEID = model.amortisationTypeId;
            data.ISINTEGRALFEE = model.isIntegral;
            data.INCLUDECUTOFFDAY = model.includeCutOffDay;
            data.CUTOFFDAY = model.cutOffDay;
            data.OPERATIONID = model.operationId;
            data.AMOUNT = model.amount;
            data.RATE = model.rate;
            data.FEETYPEID = model.feeTypeId;


            data.RECURRING = model.recurring;
            data.PRIMARYTAXID = model.primaryTaxId;
            data.SECONDARYTAXID = model.secondaryTaxId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            var notRemoved = model.ranges.Select(range => range.chargeRangeId).ToArray();
            context.TBL_CHARGE_RANGE.RemoveRange(
                context.TBL_CHARGE_RANGE.Where(range => !notRemoved.Contains(range.CHARGERANGEID) && range.CHARGEFEEID == chargeFeeId)
            );

            var count = model.ranges.Count();

            foreach (var range in model.ranges)
            {
                if (range.chargeRangeId <= 0)
                {
                    context.TBL_CHARGE_RANGE.Add(new TBL_CHARGE_RANGE
                    {
                        CHARGEFEEID = chargeFeeId,
                        MINIMUM = range.minimum,
                        MAXIMUM = range.maximum,
                        RATE = range.rate,
                        AMOUNT = range.amount,
                        MINIMUMANDABOVE = range.minimumAndAbove,
                        MAXIMUMANDBELOW = range.maximumAndBelow,
                        CREATEDBY = (int)model.createdBy,
                        DATETIMECREATED = general.GetApplicationDate()
                    });
                }
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChargeFeeUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated ChargeFee '{ data.CHARGEFEENAME }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ChargeFeeViewModel> GetAllChargeFee()
        {
            return this.context.TBL_CHARGE_FEE.Where(x => x.DELETED == false).Select(x => new ChargeFeeViewModel
            {
                chargeFeeId = x.CHARGEFEEID,
                chargeName = x.CHARGEFEENAME,
                accountCategoryId = x.ACCOUNTCATEGORYID,
                accountCategoryName = x.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                frequencyTypeId = x.FEEINTERVALID,
                frequencyTypeName = x.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                productId = x.PRODUCTTYPEID,
                targetId = x.FEETARGETID,
                targetName = x.TBL_FEE_TARGET.FEETARGETNAME,
                ledgerAccountId = x.GLACCOUNTID,
                amortisationTypeId = x.FEEAMORTISATIONTYPEID,
                amortizationTypeName = x.TBL_FEE_AMORTISATION_TYPE.FEEAMORTISATIONTYPENAME,
                isIntegral = x.ISINTEGRALFEE,
                includeCutOffDay = x.INCLUDECUTOFFDAY,
                cutOffDay = x.CUTOFFDAY,
                operationId = x.OPERATIONID,
                amount = x.AMOUNT,
                rate = x.RATE,
                feeTypeId = x.FEETYPEID,
                recurring = (bool)x.RECURRING,
                primaryTaxId = x.PRIMARYTAXID,
                secondaryTaxId = x.SECONDARYTAXID,
                ranges = context.TBL_CHARGE_RANGE.Where(r => r.CHARGEFEEID == x.CHARGEFEEID)
                    .Select(r => new ChargeRangeViewModel {
                        chargeRangeId = r.CHARGERANGEID,
                        minimum = r.MINIMUM,
                        maximum = r.MAXIMUM,
                        amount = r.AMOUNT,
                        rate = r.RATE,
                        minimumAndAbove = r.MINIMUMANDABOVE,
                        maximumAndBelow = r.MAXIMUMANDBELOW,
                        chargeFeeId = r.CHARGEFEEID
                    }).ToList(),
            });
        }

        public ChargeFeeViewModel GetChargeFee(int chargeProductFeeId)
        {
            var data = this.context.TBL_CHARGE_FEE.Find(chargeProductFeeId);

            if (data == null)
            {
                return null;
            }

            return new ChargeFeeViewModel
            {
                chargeFeeId = data.CHARGEFEEID,
                chargeName = data.CHARGEFEENAME,
                accountCategoryId = data.ACCOUNTCATEGORYID,
                frequencyTypeId = data.FEEINTERVALID,
                productId = data.PRODUCTTYPEID,
                targetId = data.FEETARGETID,
                ledgerAccountId = data.GLACCOUNTID,
                amortisationTypeId = data.FEEAMORTISATIONTYPEID,
                isIntegral = data.ISINTEGRALFEE,
                includeCutOffDay = data.INCLUDECUTOFFDAY,
                cutOffDay = data.CUTOFFDAY,
                operationId = data.OPERATIONID,
                amount = data.AMOUNT,
                rate = data.RATE,
                feeTypeId = data.FEETYPEID,

                recurring = (bool)data.RECURRING,
                primaryTaxId = data.PRIMARYTAXID,
                secondaryTaxId = data.SECONDARYTAXID,
                ranges = context.TBL_CHARGE_RANGE.Where(r => r.CHARGEFEEID == data.CHARGEFEEID)
                    .Select(r => new ChargeRangeViewModel
                    {
                        chargeRangeId = r.CHARGERANGEID,
                        minimum = r.MINIMUM,
                        maximum = r.MAXIMUM,
                        amount = r.AMOUNT,
                        rate = r.RATE,
                        minimumAndAbove = r.MINIMUMANDABOVE,
                        maximumAndBelow = r.MAXIMUMANDBELOW,
                        chargeFeeId = r.CHARGEFEEID
                    }).ToList(),
            };
        }

        public IEnumerable<ChargeFeeViewModel> GetAllChargeFeeByCompanyId(int companyId)
        {
            return this.GetAllChargeFee().Where(x => x.companyId == companyId);
        }

        public bool DeleteChargeFee(int chargeProductFeeId, UserInfo user)
        {
            var data = this.context.TBL_CHARGE_FEE.Find(chargeProductFeeId);
            if (data == null)
            {
                return false;
            }

            data.DELETED = true;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChargeFeeDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted ChargeFee '{ data.CHARGEFEENAME }' ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

    }
}