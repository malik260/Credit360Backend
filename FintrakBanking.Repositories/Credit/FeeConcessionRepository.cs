using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Credit
{
    public class FeeConcessionRepository : IFeeConcessionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;
        public FeeConcessionRepository(FinTrakBankingContext _context, IApprovalLevelStaffRepository _level,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail, IWorkflow _workFlow)
        {
            context = _context;
            this._genSetup = genSetup;
            this.auditTrail = _auditTrail;
            this.workFlow = _workFlow;
            this.level = _level;
        }
        public IEnumerable<FeeConcessionTypeViewModel> GetConcessionFeeType()
        {
            var type = (from a in context.TBL_LOAN_CONCESSION_TYPE
                        select new FeeConcessionTypeViewModel()
                        {
                            concessionTypeId = a.CONCESSIONTYPEID,
                            concessionTypeName = a.CONCESSIONTYPENAME
                        }).ToList();
            return type;
        }

        public IEnumerable<LoanFeeChargesViewModel> GetAllLoanFeeChargeByDetailId(int loanApplicationDetailId)
        {
            var type = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                        where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                        select new LoanFeeChargesViewModel()
                        {
                            loanChargeFeeId = a.LOANCHARGEFEEID,
                            chargesId = a.CHARGEFEEID,
                            chargesTypeName = a.TBL_CHARGE_FEE.CHARGEFEENAME,
                            defaultValue = a.DEFAULT_FEERATEVALUE
                        }).ToList();
            return type;
        }

        public IEnumerable<FeeConcessionViewModel> GetAllConcessionFee(int loanApplicationDetailId)
        {
            var feeConcession = (from a in context.TBL_LOAN_RATE_FEE_CONCESSION
                                 where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                 orderby a.DATETIMECREATED descending
                                 select new FeeConcessionViewModel()
                                 {
                                     concessionId = a.CONCESSIONID,
                                     concessionTypeId = a.CONCESSIONTYPEID,
                                     concessionTypeName = a.TBL_LOAN_CONCESSION_TYPE.CONCESSIONTYPENAME,
                                     concessionReason = a.CONSESSIONREASON,
                                     concession = a.CONCESSION,
                                     loanChargeFeeId = a.LOANCHARGEFEEID,
                                     loanChargeFeeName = a.TBL_LOAN_APPLICATION_DETL_FEE.TBL_CHARGE_FEE.CHARGEFEENAME,
                                     loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                     loanRefNo = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                     approvalStatusId = a.APPROVALSTATUSID,
                                     approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                     defaultValue = a.LOANCHARGEFEEID == null ? a.TBL_LOAN_APPLICATION_DETAIL.PROPOSEDINTERESTRATE : (double)context.TBL_LOAN_APPLICATION_DETL_FEE.FirstOrDefault(x => x.LOANCHARGEFEEID == a.LOANCHARGEFEEID).DEFAULT_FEERATEVALUE
                                 }).ToList();
            return feeConcession;
        }
        public IEnumerable<FeeConcessionViewModel> GetAllConcessionFeeAwaitingApproval(int staffId, int companyId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.FeeConcessionApproval).ToList();
            //var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId);
            //int staffApprovalLevelId = 0;
            //if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var feeConcession = (from a in context.TBL_LOAN_RATE_FEE_CONCESSION
                                 join atrail in context.TBL_APPROVAL_TRAIL on a.CONCESSIONID equals atrail.TARGETID
                                 where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                 && atrail.OPERATIONID == (int)OperationsEnum.FeeConcessionApproval
                                 && ids.Contains((int)atrail.TOAPPROVALLEVELID) //atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                                 && atrail.RESPONSESTAFFID == null
                                 orderby a.DATETIMECREATED descending
                                 select new FeeConcessionViewModel()
                                 {
                                     concessionId = a.CONCESSIONID,
                                     concessionTypeId = a.CONCESSIONTYPEID,
                                     concessionTypeName = a.TBL_LOAN_CONCESSION_TYPE.CONCESSIONTYPENAME,
                                     concessionReason = a.CONSESSIONREASON,
                                     concession = a.CONCESSION,
                                     loanChargeFeeId = a.LOANCHARGEFEEID,
                                     loanChargeFeeName = a.TBL_LOAN_APPLICATION_DETL_FEE.TBL_CHARGE_FEE.CHARGEFEENAME,
                                     loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                     loanRefNo = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                     approvalStatusId = a.APPROVALSTATUSID,
                                     approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                     defaultValue = a.LOANCHARGEFEEID == null ? a.TBL_LOAN_APPLICATION_DETAIL.PROPOSEDINTERESTRATE : (double)context.TBL_LOAN_APPLICATION_DETL_FEE.FirstOrDefault(x => x.LOANCHARGEFEEID == a.LOANCHARGEFEEID).DEFAULT_FEERATEVALUE
                                 }).ToList();
            return feeConcession;
        }
        public int AddUpdateFeeConcession(FeeConcessionViewModel model)
        {
            if (model == null) return 0;
            TBL_LOAN_RATE_FEE_CONCESSION data;
            if (model.concessionId > 0)
            {
                data = context.TBL_LOAN_RATE_FEE_CONCESSION.Find(model.concessionId);
                if (data != null)
                {
                    data.CONCESSION = model.concession;
                    data.CONCESSIONTYPEID = (short)model.concessionTypeId;
                    data.CONSESSIONREASON = model.concessionReason;
                    data.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
                    data.LOANCHARGEFEEID = model.loanChargeFeeId;
                    data.DATETIMEUPDATED = DateTime.Now;
                    data.LASTUPDATEDBY = model.createdBy;
                }
            }
            else
            {
                data = new TBL_LOAN_RATE_FEE_CONCESSION();
                data.CONCESSION = model.concession;
                data.CONCESSIONTYPEID = (short)model.concessionTypeId;
                data.CONSESSIONREASON = model.concessionReason;
                data.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
                data.LOANCHARGEFEEID = model.loanChargeFeeId;
                data.DELETED = false;
                data.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                data.DATETIMECREATED = _genSetup.GetApplicationDate();
                data.CREATEDBY = (int)model.createdBy;
            }
            //Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added new Fee Concession",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),    
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                 URL = model.applicationUrl, 
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            //Log to the approval workflow 
            if (model.concessionId == 0)
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.TBL_LOAN_RATE_FEE_CONCESSION.Add(data);
                        this.auditTrail.AddAuditTrail(audit);
                        var output = context.SaveChanges() > 0;

                        var entity = new ApprovalViewModel
                        {
                            staffId = model.createdBy,
                            companyId = model.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = data.CONCESSIONID,
                            operationId = (int)OperationsEnum.FeeConcessionApproval,
                            BranchId = model.userBranchId,
                            externalInitialization = true
                        };
                        var returnVal = workFlow.LogForApproval(entity);
                        if (returnVal)
                        {
                            trans.Commit();
                            return data.CONCESSIONID;
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return 0;
                        throw new SecureException(ex.Message);
                    }
                }
            }
            else
            {
                if (context.SaveChanges() > 0)
                    return data.CONCESSIONID;
            }
            return 0;
        }

        public int GoForApproval(ApprovalViewModel entity)
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
                    workFlow.ExternalInitialization = false;
                    workFlow.OperationId = (int)OperationsEnum.FeeConcessionApproval;
                    workFlow.LogActivity();

                    // workFlow.LogForApproval(entity);

                    if (entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                    {
                        var feeConcessionRecord = (from s in context.TBL_LOAN_RATE_FEE_CONCESSION
                                                   where s.CONCESSIONID == entity.targetId
                                                   select s).FirstOrDefault();
                        if(feeConcessionRecord != null)
                        {
                            feeConcessionRecord.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                            context.SaveChanges();
                            trans.Commit();
                            return 2;
                        }    
                    }


                    var b = workFlow.NextLevelId ?? 0;
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new SecureException("Approval Failed");
                    }
                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveFeeConcession(entity.targetId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return 1;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }
        private bool ApproveFeeConcession(int targetId, ApprovalViewModel user)
        {
            bool output = false;
            var feeConcessionRecord = (from s in context.TBL_LOAN_RATE_FEE_CONCESSION
                                       where s.CONCESSIONID == targetId
                                      && s.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                       select s).FirstOrDefault();
            TBL_LOAN_APPLICATION_DETAIL loanDetailRecord = null;
            TBL_LOAN_APPLICATION_DETL_FEE feeRecord = null;
            if (feeConcessionRecord.CONCESSIONTYPEID == (short)FeeConcessionTypeEnum.Interest)
            {
                loanDetailRecord = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                                    where a.LOANAPPLICATIONDETAILID == feeConcessionRecord.LOANAPPLICATIONDETAILID
                                    select a).FirstOrDefault();
            }
            else if (feeConcessionRecord.CONCESSIONTYPEID == (short)FeeConcessionTypeEnum.Fee && feeConcessionRecord.LOANCHARGEFEEID != null)
            {
                feeRecord = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                             where a.LOANAPPLICATIONDETAILID == feeConcessionRecord.LOANAPPLICATIONDETAILID
                             && a.LOANCHARGEFEEID == feeConcessionRecord.LOANCHARGEFEEID
                             select a).FirstOrDefault();
            }


            if (workFlow.NewState != (int)ApprovalState.Ended)
            {
                feeConcessionRecord.APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing;
            }
            else if (workFlow.NewState == (int)ApprovalState.Ended)
            {
                feeConcessionRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                if (loanDetailRecord != null && feeConcessionRecord.CONCESSIONTYPEID == (short)FeeConcessionTypeEnum.Interest)
                {
                    loanDetailRecord.APPROVEDINTERESTRATE = feeConcessionRecord.CONCESSION;
                }
                else if (feeRecord != null && feeConcessionRecord.CONCESSIONTYPEID == (short)FeeConcessionTypeEnum.Fee)
                {
                    feeRecord.HASCONSESSION = true;
                    feeRecord.CONSESSIONREASON = feeConcessionRecord.CONSESSIONREASON;
                    feeRecord.RECOMMENDED_FEERATEVALUE = (decimal)feeConcessionRecord.CONCESSION;
                    feeRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                }
            }
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanChecklistUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approve fee concession with concessionId: {targetId}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //output = context.SaveChanges() > 0;
            try
            {
                output = context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
            return output;
        }
        public bool ValidateFeeConcession(int loanApplicationDetailId, int? loanChargeFeeId)
        {
            bool returnVal = false;
            var exist = (from a in context.TBL_LOAN_RATE_FEE_CONCESSION
                         where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId &&
                         a.LOANCHARGEFEEID == loanChargeFeeId && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                         select a).ToList();

            if (exist.Any())
            {
                returnVal = true;
            }
            return returnVal;
        }
        public bool ValidateApprovedFeeConcession(int concessionId)
        {
            bool returnVal = false;
            var isApproved = (from a in context.TBL_LOAN_RATE_FEE_CONCESSION
                              where a.CONCESSIONID == concessionId &&
                                a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                              select a).FirstOrDefault();

            if (isApproved != null)
            {
                returnVal = true;
            }
            return returnVal;
        }
    }
}
