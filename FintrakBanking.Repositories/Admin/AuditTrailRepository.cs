using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Admin;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Admin
{
    public class AuditTrailRepository : IAuditTrailRepository
    {
        private FinTrakBankingContext context;

        public AuditTrailRepository(FinTrakBankingContext _contex)
        {
            this.context = _contex;
        }

        public void AddAllAuditTrail(List<TBL_AUDIT> auditInput)
        {
            context.TBL_AUDIT.AddRange(auditInput);
        }

        public void AddAuditTrail(TBL_AUDIT auditInput)
        {
            context.TBL_AUDIT.Add(auditInput);
        }

        public IQueryable<AuditViewModel> GetAuditTrail(short branchId)
        {
            return from _audit in context.TBL_AUDIT
                   join atype in context.TBL_AUDIT_TYPE on _audit.AUDITTYPEID equals atype.AUDITTYPEID
                   join st in context.TBL_STAFF on _audit.STAFFID equals st.STAFFID
                   where _audit.BRANCHID == branchId
                   select new AuditViewModel
                   {
                       auditId = _audit.AUDITID,
                       applicationDate = _audit.APPLICATIONDATE,
                       auditType = atype.AUDITTYPENAME,
                       details = _audit.DETAIL,
                       firstName = st.FIRSTNAME,
                       lastName = st.LASTNAME,
                       systemDate = _audit.SYSTEMDATETIME,
                       url = _audit.URL
                   };
        }
    }
}