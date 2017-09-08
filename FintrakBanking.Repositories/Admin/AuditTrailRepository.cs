using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Admin;
using System.Linq;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Admin
{ 
    public class AuditTrailRepository : IAuditTrailRepository
    {
        private FinTrakBankingContext context;
        
        public AuditTrailRepository(FinTrakBankingContext _contex)
        {
            
            this.context = _contex;
            
        }
        public void AddAllAuditTrail(List<tbl_Audit> auditInput)
        {

            context.tbl_Audit.AddRange(auditInput);

        }
        public void AddAuditTrail(tbl_Audit  auditInput)
        {
                      
            context.tbl_Audit.Add(auditInput);

        }

        public IQueryable<AuditViewModel> GetAuditTrail(short branchId)
        {
            return from _audit in context.tbl_Audit
                        join atype in context.tbl_Audit_Type
on _audit.AuditTypeId equals atype.AuditTypeId
                        join st in context.tbl_Staff  on _audit.StaffId equals st.StaffId
                   where _audit.BranchId ==branchId
                        select new AuditViewModel
                        {
                            auditId = _audit.AuditId,
                            applicationDate = _audit.ApplicationDate,
                            auditType = atype.AuditTypeName,
                            details =_audit.Detail,
                            firstName =st.FirstName,
                            lastName =st.LastName,
                            systemDate =_audit.SystemDateTime,
                            url =_audit.Url
                        };
        }
    }
}
