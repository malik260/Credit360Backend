using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Finance;

namespace FintrakBanking.Repositories.credit
{
    public class FacilityModificationRepository : IFacilityModificationRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IFinanceTransactionRepository finance;
        private ILoanApplicationRepository loanRepo;
        private IWorkflow workflow;

        public FacilityModificationRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IFinanceTransactionRepository _finance,
                ILoanApplicationRepository _loanRepo,
                IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.finance = _finance;
            this.loanRepo = _loanRepo;
            this.workflow = _workflow;
        }

        public IEnumerable<FacilityModificationViewModel> GetFacilityModificationsForApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.FacilityModificationApproval;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var reliefIds = general.GetStaffRlieved(staffId);

                var modifications = (from x in context.TBL_FACILITY_MODIFICATION
                                    join d in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                    join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                                    join t in context.TBL_APPROVAL_TRAIL on x.FACILITYMODIFICATIONID equals t.TARGETID
                                    where
                                    x.DELETED == false
                                    && t.OPERATIONID == operationId
                                    && levelIds.Contains(t.TOAPPROVALLEVELID ?? 0)
                                    && t.RESPONSESTAFFID == null
                                    && t.APPROVALSTATEID != (int)ApprovalState.Ended
                                    && (reliefIds.Contains(t.TOSTAFFID ?? 0) || t.TOSTAFFID == null)
                                    select new FacilityModificationViewModel
                                    {
                                        facilityModificationId = x.FACILITYMODIFICATIONID,
                                        loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                        approvedProductId = x.APPROVEDPRODUCTID,
                                        approvedInterestRate = x.APPROVEDINTERESTRATE,
                                        approvedTenor = x.APPROVEDTENOR,
                                        tenorModeId = x.TENORMODEID,
                                        sectorId = (int)context.TBL_SUB_SECTOR.FirstOrDefault(s => s.SUBSECTORID == x.SUBSECTORID).SECTORID,
                                        subSectorId = x.SUBSECTORID,
                                        productClassId = x.PRODUCTCLASSID,
                                        loanDetailReviewTypeId = x.LOANDETAILREVIEWTYPEID,
                                        approvedAmount = x.APPROVEDAMOUNT,
                                        customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                        applicationRef = d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                        systemArrivalDateTime = t.SYSTEMARRIVALDATETIME,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == t.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        productClassProcessId = d.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID
                                        //repaymentScheduleId 
                                        //interestRepaymentId 
                                        //repaymentTerms 
                                        //interestRepayment 
                                    }).ToList();
            return modifications;
        }
        
        public FacilityModificationViewModel GetFacilityModification(int facilityModificationId)
        {
            var entity = context.TBL_FACILITY_MODIFICATION.FirstOrDefault(x => x.FACILITYMODIFICATIONID == facilityModificationId && x.DELETED == false);

            return new FacilityModificationViewModel
            {
                facilityModificationId = entity.FACILITYMODIFICATIONID,
                loanApplicationDetailId = entity.LOANAPPLICATIONDETAILID,
                approvedProductId = entity.APPROVEDPRODUCTID,
                approvedInterestRate = entity.APPROVEDINTERESTRATE,
                approvedTenor = entity.APPROVEDTENOR,
                tenorModeId = entity.TENORMODEID,
                sectorId = (int)context.TBL_SUB_SECTOR.FirstOrDefault(s => s.SUBSECTORID == entity.SUBSECTORID).SECTORID,
                subSectorId = entity.SUBSECTORID,
                productClassId = entity.PRODUCTCLASSID,
                loanDetailReviewTypeId = entity.LOANDETAILREVIEWTYPEID,
                approvedAmount = entity.APPROVEDAMOUNT,
            };
        }

        public WorkflowResponse AddFacilityModification(FacilityModificationViewModel model)
        {

            using (var trans = context.Database.BeginTransaction())
            {
                var entity = new TBL_FACILITY_MODIFICATION
                {
                    LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                    APPROVEDPRODUCTID = model.approvedProductId,
                    APPROVEDINTERESTRATE = model.approvedInterestRate,
                    APPROVEDTENOR = model.approvedTenor,
                    TENORMODEID = model.tenorModeId,
                    SUBSECTORID = model.subSectorId,
                    PRODUCTCLASSID = model.productClassId,
                    LOANDETAILREVIEWTYPEID = model.loanDetailReviewTypeId,
                    APPROVEDAMOUNT = model.approvedAmount,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now
                };

                var save = context.TBL_FACILITY_MODIFICATION.Add(entity);
                context.SaveChanges();
                var fees = new List<TBL_FACILITY_MOD_DETL_FEE>();
                if (model.fees != null)
                {
                    foreach (var f in model.fees)
                    {
                        var existingFee = context.TBL_LOAN_APPLICATION_DETL_FEE.FirstOrDefault(c => c.LOANAPPLICATIONDETAILID == save.LOANAPPLICATIONDETAILID && c.CHARGEFEEID == f.feeId && c.DELETED == false);
                        if (existingFee != null)
                        {
                            var fee = new TBL_FACILITY_MOD_DETL_FEE
                            {
                                FACILITYMODIFICATIONID = save.FACILITYMODIFICATIONID,
                                LOANCHARGEFEEID = existingFee.LOANCHARGEFEEID,
                                CHARGEFEEID = existingFee.CHARGEFEEID,
                                DEFAULT_FEERATEVALUE = existingFee.DEFAULT_FEERATEVALUE,
                                RECOMMENDED_FEERATEVALUE = f.rate
                            };
                            fees.Add(fee);
                        }
                    }
                }

                if (fees.Count > 0)
                {
                    var save2 = context.TBL_FACILITY_MOD_DETL_FEE.AddRange(fees);
                }


                workflow.OperationId = (int)OperationsEnum.FacilityModificationApproval;
                workflow.StaffId = model.createdBy;
                workflow.TargetId = entity.FACILITYMODIFICATIONID;
                workflow.CompanyId = model.companyId;
                workflow.Vote = 2;
                workflow.StatusId = 1;
                workflow.Comment = "Kindly approve this facility modification";
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                var saved = context.SaveChanges() > 0;
                //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                //// Audit Section ---------------------------
                //this.audit.AddAuditTrail(new TBL_AUDIT
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.FacilityModificationAdded,
                //    STAFFID = model.createdBy,
                //    BRANCHID = (short)model.userBranchId,
                //    DETAIL = $"TBL_Lc Condition '{entity.ToString()}' created by {auditStaff}",
                //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                //    APPLICATIONDATE = general.GetApplicationDate(),
                //    URL = model.applicationUrl,
                //    SYSTEMDATETIME = DateTime.Now,
                //    DEVICENAME = CommonHelpers.GetDeviceName(),
                //    OSNAME = CommonHelpers.FriendlyName()
                //});
                // Audit Section end ------------------------
                if (saved)
                {
                    trans.Commit();
                    return workflow.Response;
                }
                trans.Rollback();
                return workflow.Response;
            }
        }

        public WorkflowResponse ApproveFacilityModification(ForwardViewModel model)
        {
            using (var trans = this.context.Database.BeginTransaction())
            {
                bool saved;
                var modification = context.TBL_FACILITY_MODIFICATION.Find(model.targetId);
                if (modification == null)
                {
                    throw new SecureException("Facility Modification doesn't exist!");
                }
                workflow.OperationId = (int)OperationsEnum.FacilityModificationApproval;
                workflow.StaffId = model.createdBy;
                workflow.TargetId = model.targetId;
                workflow.CompanyId = model.companyId;
                workflow.Vote = 2;
                workflow.StatusId = model.forwardAction;
                workflow.Comment = model.comment;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                saved = context.SaveChanges() > 0;
                if (workflow.NewState == (int)ApprovalState.Ended)
                {
                    if(workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                    {
                        modification.APPROVALSTATUSID = (short)workflow.StatusId;
                        var modified = ModifyFacility(modification);
                    }
                    else
                    {
                        modification.APPROVALSTATUSID = (short)workflow.StatusId;
                    }
                    modification.DATETIMEUPDATED = DateTime.Now;
                    modification.LASTUPDATEDBY = model.createdBy;
                }
                saved = context.SaveChanges() > 0;
                if (saved)
                {
                    trans.Commit();
                }
                else
                {
                    trans.Rollback();
                }
            }
            return workflow.Response;
        }

        public bool UpdateLoanDetailFees(List<TBL_FACILITY_MOD_DETL_FEE> fees, int createdBy)
        {

            foreach (var fee in fees)
            {
                var savedFee = context.TBL_LOAN_APPLICATION_DETL_FEE.FirstOrDefault(c => c.LOANCHARGEFEEID == fee.LOANCHARGEFEEID);
                if (savedFee != null)
                {
                    savedFee.RECOMMENDED_FEERATEVALUE = fee.RECOMMENDED_FEERATEVALUE;
                }
            }
            return context.SaveChanges() > 0;
        }

        private int ConvertTenorToDays(int proposedTenor, int? tenorModeId = 1)
        {
            int tenor = 0;
            switch (tenorModeId) // UPDATED
            {
                case (int)TenorMode.Daily: tenor = proposedTenor; break;
                case (int)TenorMode.Monthly: tenor = proposedTenor * 30; break;
                case (int)TenorMode.Yearly: tenor = proposedTenor * 365; break;
            }
            return tenor;
        }

        private int ConvertTenorDaysToTenor(int proposedTenor, int? tenorModeId = 1)
        {
            int tenor = 0;
            switch (tenorModeId) // UPDATED
            {
                case (int)TenorMode.Daily: tenor = proposedTenor; break;
                case (int)TenorMode.Monthly: tenor = proposedTenor / 30; break;
                case (int)TenorMode.Yearly: tenor = proposedTenor / 365; break;
            }
            return tenor;
        }

        public bool ModifyFacility(TBL_FACILITY_MODIFICATION model)
        {
            bool saved;
            var fees = context.TBL_FACILITY_MOD_DETL_FEE.Where(f => f.FACILITYMODIFICATIONID == model.FACILITYMODIFICATIONID).ToList();
            var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.LOANAPPLICATIONDETAILID);
            var loan = context.TBL_LOAN_APPLICATION.Find(facility.LOANAPPLICATIONID);
            var formerApplicationAmount = loan.TBL_LOAN_APPLICATION_DETAIL.Sum(d => d.APPROVEDAMOUNT * (decimal)d.EXCHANGERATE);
            if (facility != null && loan != null)
            {
                if (fees != null)
                {
                    if (fees.Count > 0)
                    {
                        var updated = UpdateLoanDetailFees(fees, model.CREATEDBY);
                    }
                }
                //var currencyRate = (finance.GetExchangeRate(facility.DATETIMECREATED, facility.CURRENCYID, loan.COMPANYID).sellingRate);
                var difference = (model.APPROVEDAMOUNT - facility.APPROVEDAMOUNT) * (decimal)facility.EXCHANGERATE;
                saved = loanRepo.ArchiveLoanApplication(facility.LOANAPPLICATIONID, (int)OperationsEnum.FacilityModificationApproval, 0, model.CREATEDBY);
                model.APPROVEDTENOR = ConvertTenorToDays(model.APPROVEDTENOR, model.TENORMODEID);
                facility.APPROVEDPRODUCTID = model.APPROVEDPRODUCTID;
                facility.PROPOSEDPRODUCTID = model.APPROVEDPRODUCTID;
                facility.APPROVEDINTERESTRATE = model.APPROVEDINTERESTRATE;
                facility.PROPOSEDINTERESTRATE = model.APPROVEDINTERESTRATE;
                facility.APPROVEDTENOR = model.APPROVEDTENOR;
                facility.PROPOSEDTENOR = model.APPROVEDTENOR;
                facility.TENORFREQUENCYTYPEID = model.TENORMODEID;
                facility.SUBSECTORID = model.SUBSECTORID;
                facility.LOANDETAILREVIEWTYPEID = model.LOANDETAILREVIEWTYPEID;
                facility.APPROVEDAMOUNT = model.APPROVEDAMOUNT;
                facility.PROPOSEDAMOUNT = model.APPROVEDAMOUNT;
                loan.APPLICATIONAMOUNT = formerApplicationAmount + difference;
                loan.TOTALEXPOSUREAMOUNT = loan.TOTALEXPOSUREAMOUNT > 0 ? loan.TOTALEXPOSUREAMOUNT + difference : formerApplicationAmount + difference;
                loan.DATETIMEUPDATED = DateTime.Now;
                loan.LASTUPDATEDBY = model.CREATEDBY;
            }
            saved = context.SaveChanges() > 0;
            return saved;
        }

        public bool UpdateFacilityModification(FacilityModificationViewModel model, int id, UserInfo user)
        {
            //var entity = this.context.TBL_FACILITY_MODIFICATION.Find(id);
            //entity.LCISSUANCEID = model.lcIssuanceId;
            //entity.CONDITION = model.condition;
            //entity.ISSATISFIED = model.isSatisfied;
            //entity.ISTRANSACTIONDYNAMICS = model.isTransactionDynamics;

            //entity.LASTUPDATEDBY = user.createdBy;
            //entity.DATETIMEUPDATED = DateTime.Now;

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.FacilityModificationUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Lc Condition '{entity.ToString()}' was updated by {auditStaff}",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    TARGETID = entity.LCCONDITIONID,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName()

            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteFacilityModification(int id, UserInfo user)
        {
            //var entity = this.context.TBL_FACILITY_MODIFICATION.Find(id);
            //entity.DELETED = true;
            //entity.DELETEDBY = user.createdBy;
            //entity.DATETIMEDELETED = general.GetApplicationDate();

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.FacilityModificationDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Lc Condition '{entity.ToString()}' was deleted by {auditStaff}",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.LCCONDITIONID,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName(),

            //});
            //// Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

    }
}

// kernel.Bind<IFacilityModificationRepository>().To<FacilityModificationRepository>();
// FacilityModificationAdded = ???, FacilityModificationUpdated = ???, FacilityModificationDeleted = ???,
