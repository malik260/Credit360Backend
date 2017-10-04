using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Entities.Models;

namespace FintrakBanking.ReportObjects
{
    public class BranchInfo
    {
        //private static FinTrakBankingContext context;

        public BranchInfo()
        {
            
        }

        public static IEnumerable<BranchViewModel> GetAllBranches()
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var branches = context.tbl_Branch.Where(x => x.Deleted == false).Select(x => new BranchViewModel
            {
                branchId = x.BranchId,
                stateId = x.StateId,
                cityId = (int)x.CityId,
                companyId = x.CompanyId,
                stateName = x.tbl_State.StateName,
                cityName = context.tbl_City.FirstOrDefault(c => c.CityId == x.CityId).CityName ?? string.Empty,
                branchName = x.BranchName,
                branchCode = x.BranchCode,
                addressLine1 = x.AddressLine1,
                addressLine2 = x.AddressLine2,
                comment = x.Comment,
                deleted = x.Deleted,
            }).ToList();

            return branches; ;

        }

        public static BranchViewModel GetBranch(short id)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var branch = context.tbl_Branch.Find(id);

            if (branch != null)
            {
                return new BranchViewModel()
                {
                    branchId = branch.BranchId,
                    stateId = branch.StateId,
                    companyId = branch.CompanyId,
                    branchName = branch.BranchName,
                    branchCode = branch.BranchCode,
                    addressLine1 = branch.AddressLine1,
                    addressLine2 = branch.AddressLine2,
                    comment = branch.Comment,
                };
            }

            return new BranchViewModel();
        }
    }
}
