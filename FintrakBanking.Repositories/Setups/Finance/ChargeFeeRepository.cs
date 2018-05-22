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
        private IWorkflow workFlow;

        public ChargeFeeRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository general,
            IAuditTrailRepository _auditTrail,
            IWorkflow _workflow
            )
        {
            this.context = context;
            this.general = general;
            this.auditTrail = _auditTrail;
            workFlow = _workflow;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.UserCreation;
            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.LogForApproval(entity);
                    var b = workFlow.NextLevelId ?? 0;
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
                    }

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveChargeFee(entity.targetId, (short)workFlow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }

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
                FEEINTERVALID = chargeFeemodel.frequencyTypeId,
                PRODUCTTYPEID = chargeFeemodel.productTypeId,
                FEETARGETID = chargeFeemodel.targetId,
                FEEAMORTISATIONTYPEID = chargeFeemodel.amortisationTypeId,
                ISINTEGRALFEE = chargeFeemodel.isIntegral,
                INCLUDECUTOFFDAY = chargeFeemodel.includeCutOffDay,
                CUTOFFDAY = chargeFeemodel.cutOffDay,
                OPERATIONID = chargeFeemodel.operationId,
                AMOUNT = chargeFeemodel.amount,
                RATE = chargeFeemodel.rate,
                FEETYPEID = chargeFeemodel.feeTypeId,
                RECURRING = chargeFeemodel.recurring,
                COMPANYID = chargeFeemodel.companyId,
                CREATEDBY = (int)chargeFeemodel.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),

            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreateStaffInitiated,
                STAFFID = chargeFeemodel.createdBy,
                BRANCHID = (short)chargeFeemodel.userBranchId,
                DETAIL = "", // $"Initiated Staff Creation for '{staffModel.StaffFullName}' with code'{staffModel.StaffCode}'",
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
            if (model != null)
            {
                try
                {
                    bool output = false;
                    List<TBL_TEMP_CHARGE_FEE_DETAIL> tempFeeDetail = new List<TBL_TEMP_CHARGE_FEE_DETAIL>();
                    if (model.chargeFeeDetails.Count > 0)
                    {
                        foreach (var item in model.chargeFeeDetails)
                        {
                            var charFeeDetail = new TBL_TEMP_CHARGE_FEE_DETAIL()
                            {
                                DESCRIPTION = item.description,
                                GLACCOUNTID1 = item.glAccountId1,
                                GLACCOUNTID2 = item.glAccountId2,
                                DETAILTYPEID = item.detailTypeId,
                                POSTINGTYPEID = item.postingTypeId,
                                VALUE = item.rate,
                                FEETYPEID = item.feeTypeId,
                                REQUIREAMORTISATION = item.requireAmortization,
                                POSTINGGROUP = item.postingGroup,
                                CREATEDBY = model.createdBy,
                                DATETIMECREATED = DateTime.Now,
                                DELETED = false,
                                TEMPCHARGEFEEID = item.chargeFeeId
                            };
                            tempFeeDetail.Add(charFeeDetail);
                        }
                    }
                    if (model.ranges.Count > 0)
                    {
                        foreach (var range in model.ranges)
                        {
                            context.TBL_CHARGE_RANGE.Add(new TBL_CHARGE_RANGE
                            {
                                MINIMUM = range.minimum,
                                MAXIMUM = range.maximum,
                                RATE = range.rate,
                                AMOUNT = range.amount,
                                MINIMUMANDABOVE = range.minimumAndAbove,
                                MAXIMUMANDBELOW = range.maximumAndBelow,
                                CREATEDBY = (int)model.createdBy,
                                DATETIMECREATED = general.GetApplicationDate(),
                                CHARGEFEEID = range.chargeFeeId,
                                DELETED = false
                            });
                        }
                    }
                    TBL_TEMP_CHARGE_FEE temChargeFee;
                    if (model.chargeFeeId > 0)
                    {
                        temChargeFee = context.TBL_TEMP_CHARGE_FEE.Find(model.chargeFeeId);
                        if (temChargeFee != null)
                        {

                        }
                    }
                    else
                    {
                        temChargeFee = new TBL_TEMP_CHARGE_FEE()
                        {
                            CHARGEFEENAME = model.chargeName,
                            FEEINTERVALID = model.frequencyTypeId,
                            PRODUCTTYPEID = model.productTypeId,
                            FEETARGETID = model.targetId,
                            FEEAMORTISATIONTYPEID = model.amortisationTypeId,
                            ISINTEGRALFEE = model.isIntegral,
                            INCLUDECUTOFFDAY = model.includeCutOffDay,
                            CUTOFFDAY = model.cutOffDay,
                            OPERATIONID = model.operationId,
                            AMOUNT = model.amount,
                            RATE = model.rate,
                            FEETYPEID = model.feeTypeId,
                            RECURRING = model.recurring,
                            COMPANYID = model.companyId,
                            CREATEDBY = (int)model.createdBy,
                            DATETIMECREATED = general.GetApplicationDate(),
                            APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                            ISCURRENT = true,
                            // TBL_TEMP_CHARGE_FEE_DETAIL = tempFeeDetail,
                            DELETED = false,
                            ISUPDATESTATUS = false
                        };
                        context.TBL_TEMP_CHARGE_FEE.Add(temChargeFee);
                    }
                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ChargeFeeAdded,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = $"Added Charge Fee '{temChargeFee.CHARGEFEENAME }' ",
                        IPADDRESS = model.userIPAddress,
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = model.targetId
                    };
                    using (var trans = context.Database.BeginTransaction())
                    {
                        try
                        {

                            this.auditTrail.AddAuditTrail(audit);
                            output = context.SaveChanges() > 0;

                            if (output == true)
                            {
                                foreach (var item in tempFeeDetail)
                                {
                                    item.TEMPCHARGEFEEID = temChargeFee.TEMPCHARGEFEEID;
                                }
                                context.TBL_TEMP_CHARGE_FEE_DETAIL.AddRange(tempFeeDetail);
                                output = context.SaveChanges() > 0;
                            }

                            workFlow.StaffId = model.createdBy;
                            workFlow.CompanyId = model.companyId;
                            workFlow.StatusId = (int)ApprovalStatusEnum.Pending;
                            workFlow.TargetId = temChargeFee.TEMPCHARGEFEEID;
                            workFlow.Comment = "Charge Fee Creation";
                            workFlow.OperationId = (int)OperationsEnum.FeeCreation;
                            workFlow.ExternalInitialization = true;

                            var response = workFlow.LogActivity();

                            if (response)
                            {
                                trans.Commit();
                            }
                            return output;
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new Exception(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return false;
        }

        public bool UpdateChargeFee(ChargeFeeViewModel model, int chargeFeeId)
        {
            var data = this.context.TBL_CHARGE_FEE.Find(chargeFeeId);
            if (data == null)
            {
                return false;
            }

            data.CHARGEFEENAME = model.chargeName;
            data.FEEINTERVALID = model.frequencyTypeId;
            data.PRODUCTTYPEID = model.productTypeId;
            data.FEETARGETID = model.targetId;
            data.FEEAMORTISATIONTYPEID = model.amortisationTypeId;
            data.ISINTEGRALFEE = model.isIntegral;
            data.INCLUDECUTOFFDAY = model.includeCutOffDay;
            data.CUTOFFDAY = model.cutOffDay;
            data.OPERATIONID = model.operationId;
            data.AMOUNT = model.amount;
            data.RATE = model.rate;
            data.FEETYPEID = model.feeTypeId;
            data.RECURRING = model.recurring;
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
                frequencyTypeId = x.FEEINTERVALID,
                frequencyTypeName = x.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                productTypeId = x.PRODUCTTYPEID,
                targetId = x.FEETARGETID,
                targetName = x.TBL_FEE_TARGET.FEETARGETNAME,
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
                ranges = context.TBL_CHARGE_RANGE.Where(r => r.CHARGEFEEID == x.CHARGEFEEID)
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
                frequencyTypeId = data.FEEINTERVALID,
                productTypeId = data.PRODUCTTYPEID,
                targetId = data.FEETARGETID,
                amortisationTypeId = data.FEEAMORTISATIONTYPEID,
                isIntegral = data.ISINTEGRALFEE,
                includeCutOffDay = data.INCLUDECUTOFFDAY,
                cutOffDay = data.CUTOFFDAY,
                operationId = data.OPERATIONID,
                amount = data.AMOUNT,
                rate = data.RATE,
                feeTypeId = data.FEETYPEID,
                recurring = (bool)data.RECURRING,
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

        public IEnumerable<LookupViewModel> GetAllChargeFeeDetailClass()
        {
            //return context.TBL_CHARGE_FEE_DETAIL_CLASS.Select(x => new LookupViewModel()
            //{
            //    lookupId = x.DETAILCLASSID,
            //    lookupName = x.DETAILCLASSNAME
            //});
            return new List<LookupViewModel>();
        }
        public IEnumerable<LookupViewModel> GetAllChargeFeeDetailType()
        {
            return context.TBL_CHARGE_FEE_DETAIL_TYPE.Select(x => new LookupViewModel()
            {
                lookupId = x.DETAILTYPEID,
                lookupName = x.DETAILTYPENAME
            });
        }
        public IEnumerable<LookupViewModel> GetAllFeeType()
        {
            return context.TBL_FEE_TYPE.Select(x => new LookupViewModel()
            {
                lookupId = x.FEETYPEID,
                lookupName = x.FEETYPENAME
            });
        }
        public IEnumerable<LookupViewModel> GetAllPostingType()
        {
            return context.TBL_POSTING_TYPE.Select(x => new LookupViewModel()
            {
                lookupId = x.POSTINGTYPEID,
                lookupName = x.POSTINGTYPENAME
            });
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