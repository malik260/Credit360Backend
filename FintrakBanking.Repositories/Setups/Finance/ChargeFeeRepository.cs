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
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;

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
            IGeneralSetupRepository _general,
            IAuditTrailRepository _auditTrail,
            IWorkflow _workflow
            )
        {
            this.context = context;
            this.general = _general;
            this.auditTrail = _auditTrail;
            workFlow = _workflow;
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
                throw new SecureException("Charge Fee Information already exist and is undergoing approval");
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = chargeFeemodel.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                            throw new SecureException("Approval route have not been defined for this operation");
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
                            TBL_TEMP_CHARGE_FEE_DETAIL = tempFeeDetail,
                            DELETED = false,
                            ISUPDATESTATUS = false,
                            CRMSREGULATORYID = model.crmsRegulatoryId,
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
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = model.targetId,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };
                    using (var trans = context.Database.BeginTransaction())
                    {
                        try
                        {

                            this.auditTrail.AddAuditTrail(audit);
                            output = context.SaveChanges() > 0;

                            //if (output == true)
                            //{
                            //    foreach (var item in tempFeeDetail)
                            //    {
                            //        item.TEMPCHARGEFEEID = temChargeFee.TEMPCHARGEFEEID;
                            //    }
                            //    context.TBL_TEMP_CHARGE_FEE_DETAIL.AddRange(tempFeeDetail);
                            //    output = context.SaveChanges() > 0;
                            //}

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
                            throw new SecureException(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
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
            data.CRMSREGULATORYID = model.crmsRegulatoryId;
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------
             
            return context.SaveChanges() != 0;
        }

        public IEnumerable<ChargeFeeViewModel> GetChargeFeeAwaitingApprovals(int staffId, int companyId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.Fee_chargeChange).ToList();

            var result = (from a in context.TBL_LOAN_REVIEW_OPERATION
                          join t in context.TBL_APPROVAL_TRAIL on a.LOANREVIEWOPERATIONID equals t.TARGETID
                          where (t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                            && t.RESPONSESTAFFID == null
                            && t.OPERATIONID == (int)OperationsEnum.Fee_chargeChange
                          && ids.Contains((int)t.TOAPPROVALLEVELID)
                          select new ChargeFeeViewModel
                          {
                              chargeName = context.TBL_CHARGE_FEE.Where(O => O.CHARGEFEEID == a.INTERESTFREQUENCYTYPEID).FirstOrDefault().CHARGEFEENAME,
                              operationId = a.OPERATIONTYPEID,
                              amount = a.PREPAYMENT,
                              rate = a.INTERATERATE,
                              chargeFeeId = (short) a.INTERESTFREQUENCYTYPEID,
                              loanChargeFeeId = a.PRINCIPALFREQUENCYTYPEID,
                              loanReviewOperationId = a.LOANREVIEWOPERATIONID,
                              loanReviewApplicationId = a.LOANREVIEWAPPLICATIONID,
                              loanId = a.LOANID,
                              loanSystemTypeId = a.LOANSYSTEMTYPEID,

                              //responseApprovalLevel = t.TOAPPROVALLEVELID.HasValue ? t.TBL_APPROVAL_LEVEL1.LEVELNAME : "N/A",
                              //systemArrivalDate = t.SYSTEMARRIVALDATETIME,
                              //systemResponseDate = t.SYSTEMRESPONSEDATETIME,
                              //responseStaffName = !t.TOAPPROVALLEVELID.HasValue ? "Initiation" : t.TBL_APPROVAL_LEVEL1.LEVELNAME,
                              //comment = t.COMMENT,
                              //requestStaffName = t.TBL_STAFF.FIRSTNAME != null ? t.TBL_STAFF.FIRSTNAME + " " + t.TBL_STAFF.LASTNAME : null,
                              //requestApprovalLevel = !t.FROMAPPROVALLEVELID.HasValue ? "Initiation" : t.TBL_APPROVAL_LEVEL.LEVELNAME,
                              //approvalStatus = t.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME
                          }).ToList();

            return result;

            //var charge = (from a in context.TBL_TEMP_CHARGE_FEE
            //              join t in context.TBL_APPROVAL_TRAIL on a.TEMPCHARGEFEEID equals t.TARGETID
            //              where (t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
            //                && a.ISCURRENT == true
            //                && t.RESPONSESTAFFID == null
            //                && t.OPERATIONID == (int)OperationsEnum.FeeCreation
            //              && ids.Contains((int)t.TOAPPROVALLEVELID)
            //              select new ChargeFeeViewModel
            //              {
            //                  crmsRegulatoryId = a.CRMSREGULATORYID,
            //                  chargeFeeId = a.TEMPCHARGEFEEID,
            //                  chargeName = a.CHARGEFEENAME,
            //                  frequencyTypeId = a.FEEINTERVALID,
            //                  frequencyTypeName = a.TBL_FEE_INTERVAL.FEEINTERVALNAME,
            //                  productTypeId = a.PRODUCTTYPEID,
            //                  targetId = a.FEETARGETID,
            //                  targetName = a.TBL_FEE_TARGET.FEETARGETNAME,
            //                  amortisationTypeId = a.FEEAMORTISATIONTYPEID,
            //                  amortizationTypeName = a.TBL_FEE_AMORTISATION_TYPE.FEEAMORTISATIONTYPENAME,
            //                  isIntegral = a.ISINTEGRALFEE,
            //                  includeCutOffDay = a.INCLUDECUTOFFDAY,
            //                  cutOffDay = a.CUTOFFDAY,
            //                  operationId = a.OPERATIONID,
            //                  amount = a.AMOUNT,
            //                  rate = a.RATE,
            //                  feeTypeId = a.FEETYPEID,
            //                  recurring = (bool)a.RECURRING,
            //                  chargeFeeDetails = context.TBL_TEMP_CHARGE_FEE_DETAIL.Where(q => q.TEMPCHARGEFEEID == a.TEMPCHARGEFEEID).
            //                                        Select(q => new ChargeFeeDetailsViewModel
            //                                        {
            //                                            chargeFeeDetailId = q.TEMPCHARGEFEEDETAILID,
            //                                            description = q.DESCRIPTION,
            //                                            chargeFeeId = q.TEMPCHARGEFEEID,
            //                                            glAccountId1 = q.GLACCOUNTID1,
            //                                            glAccountId2 = q.GLACCOUNTID2,
            //                                            detailTypeId = q.DETAILTYPEID,
            //                                            postingTypeId = q.POSTINGTYPEID,
            //                                            amount = q.VALUE,
            //                                            rate = q.VALUE,
            //                                            feeTypeId = q.FEETYPEID,
            //                                            requireAmortization = q.REQUIREAMORTISATION,
            //                                            postingGroup = q.POSTINGGROUP
            //                                        }).ToList(),
            //              }).ToList();

        }

        private bool ApproveChargeFee(int loanChargeFeeId, decimal feeAmount, double feeRate, short approvalStatusId, UserInfo user)
        {
            //var tempCharge = (from a in context.TBL_TEMP_CHARGE_FEE where a.TEMPCHARGEFEEID == targetId select a).FirstOrDefault();
            //var tempChargeDetails = (from a in context.TBL_TEMP_CHARGE_FEE_DETAIL where a.TEMPCHARGEFEEID == targetId select a).ToList();

            var oldChargeFee = (from a in context.TBL_CHARGE_FEE where a.CHARGEFEEID == loanChargeFeeId select a).FirstOrDefault();
            List<TBL_CHARGE_FEE_DETAIL> details = new List<TBL_CHARGE_FEE_DETAIL>();

            //if (tempChargeDetails != null)
            //{
            //    foreach (var item in tempChargeDetails)
            //    {
            //        var newDetails = new TBL_CHARGE_FEE_DETAIL()
            //        {
            //            DESCRIPTION = item.DESCRIPTION,
            //            GLACCOUNTID1 = item.GLACCOUNTID1,
            //            GLACCOUNTID2 = item.GLACCOUNTID2,
            //            DETAILTYPEID = item.DETAILTYPEID,
            //            POSTINGTYPEID = item.POSTINGTYPEID,
            //            VALUE = item.VALUE,
            //            FEETYPEID = item.FEETYPEID,
            //            REQUIREAMORTISATION = item.REQUIREAMORTISATION,
            //            POSTINGGROUP = item.POSTINGGROUP,
            //            CREATEDBY = item.CREATEDBY,
            //            DATETIMECREATED = item.DATETIMECREATED,
            //            DELETED = false
            //        };
            //        details.Add(newDetails);
            //    }
            //}

            List<TBL_CHARGE_FEE_DETAIL> targetDetails = null;

            // Removing existing details
            //if (tempCharge.CHARGEFEEID > 0)
            //{
            //    targetDetails = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == tempCharge.CHARGEFEEID).ToList();
            //    if (targetDetails.Any())
            //    {
            //        foreach (var item in targetDetails)
            //        {
            //            context.TBL_CHARGE_FEE_DETAIL.Remove(item);
            //        }
            //    }
            //}

            TBL_CHARGE_FEE targetCharge;

            if (oldChargeFee.CHARGEFEEID > 0)
            {
                //targetCharge = context.TBL_CHARGE_FEE.Find(tempCharge.CHARGEFEEID);

                //if (targetCharge != null)
                //{
                //    targetCharge.CHARGEFEENAME = tempCharge.CHARGEFEENAME;
                //    targetCharge.FEEINTERVALID = tempCharge.FEEINTERVALID;
                //    targetCharge.PRODUCTTYPEID = tempCharge.PRODUCTTYPEID;
                //    targetCharge.FEETARGETID = tempCharge.FEETARGETID;
                //    targetCharge.FEEAMORTISATIONTYPEID = tempCharge.FEEAMORTISATIONTYPEID;
                //    targetCharge.ISINTEGRALFEE = tempCharge.ISINTEGRALFEE;
                //    targetCharge.INCLUDECUTOFFDAY = tempCharge.INCLUDECUTOFFDAY;
                //    targetCharge.CUTOFFDAY = tempCharge.CUTOFFDAY;
                //    targetCharge.OPERATIONID = tempCharge.OPERATIONID;
                //    targetCharge.AMOUNT = tempCharge.AMOUNT;
                //    targetCharge.RATE = tempCharge.RATE;
                //    targetCharge.FEETYPEID = tempCharge.FEETYPEID;
                //    targetCharge.RECURRING = tempCharge.RECURRING;
                //    targetCharge.TBL_CHARGE_FEE_DETAIL = details;
                //    targetCharge.CRMSREGULATORYID = tempCharge.CRMSREGULATORYID;
                //};

                oldChargeFee.AMOUNT = oldChargeFee.AMOUNT - feeAmount;
                oldChargeFee.RATE = oldChargeFee.RATE - feeRate;
            }
            else
            {
                //targetCharge = new TBL_CHARGE_FEE()
                //{
                //    CHARGEFEENAME = tempCharge.CHARGEFEENAME,
                //    FEEINTERVALID = tempCharge.FEEINTERVALID,
                //    PRODUCTTYPEID = tempCharge.PRODUCTTYPEID,
                //    FEETARGETID = tempCharge.FEETARGETID,
                //    FEEAMORTISATIONTYPEID = tempCharge.FEEAMORTISATIONTYPEID,
                //    ISINTEGRALFEE = tempCharge.ISINTEGRALFEE,
                //    INCLUDECUTOFFDAY = tempCharge.INCLUDECUTOFFDAY,
                //    CUTOFFDAY = tempCharge.CUTOFFDAY,
                //    OPERATIONID = tempCharge.OPERATIONID,
                //    AMOUNT = tempCharge.AMOUNT,
                //    RATE = tempCharge.RATE,
                //    FEETYPEID = tempCharge.FEETYPEID,
                //    RECURRING = tempCharge.RECURRING,
                //    COMPANYID = tempCharge.COMPANYID,
                //    CREATEDBY = (int)tempCharge.CREATEDBY,
                //    DATETIMECREATED = general.GetApplicationDate(),
                //    TBL_CHARGE_FEE_DETAIL = details,
                //    CRMSREGULATORYID = tempCharge.CRMSREGULATORYID,
                    
                //};
                //context.TBL_CHARGE_FEE.Add(targetCharge);
            }

           
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.FeeApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Approved Charge Fee",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            try
            {               
                auditTrail.AddAuditTrail(audit);
                // Audit Section ---------------------------
                var response = context.SaveChanges() > 0;

                if (response)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.StaffId = entity.staffId;
                    workFlow.CompanyId = entity.companyId;
                    workFlow.StatusId = ((short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId;
                    workFlow.TargetId = entity.targetId;
                    workFlow.Comment = entity.comment;
                    workFlow.OperationId = (int)OperationsEnum.Fee_chargeChange;
                    workFlow.LogActivity();

                    var b = workFlow.NextLevelId ?? 0;

                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new SecureException("Approval Failed");
                    }

                    var loanReviewOperation = context.TBL_LOAN_REVIEW_OPERATION.Where(O => O.LOANREVIEWOPERATIONID == entity.targetId).FirstOrDefault();

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveChargeFee(entity.loanChargeFeeId, entity.feeAmount, entity.feeRate, (short)workFlow.StatusId, entity);

                        if (response)
                        {
                            if (loanReviewOperation != null) { loanReviewOperation.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved; context.SaveChanges(); }
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        //var tempCharge = (from a in context.TBL_TEMP_CHARGE_FEE where a.TEMPCHARGEFEEID == entity.targetId select a).FirstOrDefault();
                        //if (tempCharge != null)
                        //{
                        //    tempCharge.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                        //    tempCharge.ISCURRENT = true;
                        //    tempCharge.DATETIMEUPDATED = DateTime.Now;
                        //}
                        //context.SaveChanges();
                        trans.Commit();
                    } 

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        public IEnumerable<ChargeFeeViewModel> GetAllChargeFee()
        {
            var data = this.context.TBL_CHARGE_FEE.Where(x => x.DELETED == false).Select(x => new ChargeFeeViewModel
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
                crmsRegulatoryId = x.CRMSREGULATORYID,
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
                chargeFeeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(q => q.CHARGEFEEID == x.CHARGEFEEID).
                 Select(q => new ChargeFeeDetailsViewModel
                 {
                     chargeFeeDetailId = q.CHARGEFEEDETAILID,
                     description = q.DESCRIPTION,
                     chargeFeeId = q.CHARGEFEEID,
                     glAccountId1 = q.GLACCOUNTID1,
                     glAccountId2 = q.GLACCOUNTID2,
                     detailTypeId = q.DETAILTYPEID,
                     postingTypeId = q.POSTINGTYPEID,
                     amount = q.VALUE,
                     rate = q.VALUE,
                     feeTypeId = q.FEETYPEID,
                     requireAmortization = q.REQUIREAMORTISATION,
                     postingGroup = q.POSTINGGROUP
                 }).ToList(),
            });

            var test = data.ToList();

            return data;
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
                crmsRegulatoryId = data.CRMSREGULATORYID,
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
        public IEnumerable<LookupViewModel> GetAllCRMSFeeType()
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.FeeType).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupName = x.CODE + "-" + x.DESCRIPTION 
            }).ToList();
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

    }
}