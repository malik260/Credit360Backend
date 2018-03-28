using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IStaffRoleRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class StaffRoleRepository : IStaffRoleRepository
    {
        private FinTrakBankingContext context;

        public StaffRoleRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }


        public StaffRoleViewModel GetStaffRole(int jobTitleId)
        {
            var role = (from a in context.TBL_STAFF_ROLE
                        select new StaffRoleViewModel
                        {
                            staffRoleName = a.STAFFROLENAME,
                            companyId = (short)a.COMPANYID,
                            staffRoleId = a.STAFFROLEID
                        }).SingleOrDefault();
            return role;
        }
        public IEnumerable<StaffRoleViewModel> GetStaffRoleByCompanyId(int companyId)
        {
            return from a in context.TBL_STAFF_ROLE
                   where a.COMPANYID == companyId
                   select new StaffRoleViewModel
                   {
                       staffRoleName = a.STAFFROLENAME,
                       companyId = (short)a.COMPANYID,
                       staffRoleId = a.STAFFROLEID
                   };

        }

        public IEnumerable<StaffRoleViewModel> GetStaffRole()
        {
            var role = (from a in context.TBL_STAFF_ROLE
                        select new StaffRoleViewModel
                        {
                            staffRoleName = a.STAFFROLENAME,
                            companyId = (short)a.COMPANYID,
                            staffRoleId = a.STAFFROLEID
                        });
            return role;
        }

    }
}