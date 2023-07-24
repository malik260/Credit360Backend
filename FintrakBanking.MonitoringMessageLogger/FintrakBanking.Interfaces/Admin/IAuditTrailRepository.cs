
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
        void AddAuditTrail(TBL_AUDIT auditInput);

        IQueryable<AuditViewModel> GetAuditTrail(short branchId);
        IQueryable<DeletedStaffLog> GetDeletedStaffLog(short branchId);
        IQueryable<DormantStaffLog> GetDormantStaffLog(short branchId);

    }
}
