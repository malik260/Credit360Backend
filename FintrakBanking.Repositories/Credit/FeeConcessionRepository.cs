using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public IEnumerable<FeeConcessionViewModel> GetAllConcessionFee()
        {
            var feeConcession = (from a in context.TBL_LOAN_RATE_FEE_CONCESSION_
                                select new FeeConcessionViewModel()
                                {
                                    concessionId = a.CONCESSIONID,
                                    concessionTypeId = a.CONCESSIONTYPEID,
                                    concessionTypeName = a.TBL_LOAN_CONCESSION_TYPE.CONCESSIONTYPENAME,
                                    concession = a.CONSESSIONREASON,
                                    loanChargeFeeId = a.LOANCHARGEFEEID,
                                    loanChargeFeeName = a.TBL_LOAN_APPLICATION_DETL_FEE.TBL_CHARGE_FEE.CHARGEFEENAME,
                                    LoanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    approvalStatusId = a.APPROVALSTATUSID,
                                }).ToList();
            return feeConcession;
        }
    }
}
