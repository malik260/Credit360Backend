using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IBranchRepository
    {
        BranchViewModel GetBranch(short id);

        IEnumerable<BranchViewModel> GetAllBranch();

        IEnumerable<BranchViewModel> GetAllBranchByCompanyId(int id);

        Task<bool> AddBranch(AddBranchViewModel model);

        Task<bool> UpdateBranch(BranchViewModel model, short id);

        Task<bool> DeleteBranch(short id, UserInfo user);


        IEnumerable<BranchRegionViewModel> GetAllRegion();
        bool AddUpdateBranchRegion(BranchRegionViewModel entity);
        bool ValidateRegionName(string regionName);
        // tbl_Branch Limit
        //BranchLimitViewModel GetBranchLimit(short id);

        //IEnumerable<BranchLimitViewModel> GetAllBranchLimit();

        //IEnumerable<BranchLimitViewModel> GetAllBranchLimitByBranchId(int id);

        //Task<bool> AddBranchLimit(BranchLimitViewModel model);

        //Task<bool> UpdateBranchLimit(BranchLimitViewModel model);

        //Task<bool> DeleteBranchLimit(short id);
    }
}