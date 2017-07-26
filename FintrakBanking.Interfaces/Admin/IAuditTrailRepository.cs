
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FintrakBanking.Interfaces.Admin
{
    public interface IAuditTrailRepository
    {
        void AddAuditTrail(tbl_Audit auditInput);

        IQueryable<AuditViewModel> GetAuditTrail(short branchId);
    }
}
