using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IBranchRepository
    {
        Task<bool> DeleteRetailCollectionCronJobAsync(short id, UserInfo user);
        Task<bool> DeleteRetailCollectionComputationVariablesAsync(short id, UserInfo user);
        bool UpdateCollectionRetailCronJob(CollectionsRetailCronSetupViewModel model, short id);
        Task<bool> AddRetailCollectionCronJobAsync(CollectionsRetailCronSetupViewModel model);
        IEnumerable<CollectionsRetailCronSetupViewModel> GetCollectionRetailCronJobSetup();
        IEnumerable<CollectionsRetailComputationVariableSetupViewModel> GetCollectionRetailComputationVariablesSetup();
        BranchViewModel GetBranch(short id);

        IEnumerable<BranchViewModel> GetAllBranch();

        IEnumerable<BranchViewModel> GetAllBranchByCompanyId(int id);

        Task<bool> AddBranch(AddBranchViewModel model);

        Task<bool> UpdateBranch(BranchViewModel model, short id);

        Task<bool> DeleteBranch(short id, UserInfo user);


        IEnumerable<BranchRegionViewModel> GetAllRegion();

        bool AddUpdateBranchRegion(BranchRegionViewModel entity);

        IEnumerable<BranchRegionStaffViewModel> GetAllRegionStaff(int regionId);
        bool AddUpdateBranchRegionStaff(BranchRegionStaffViewModel entity);
        IEnumerable<LookupViewModel> GetAllRegionStaffType();
        Task<bool> DeleteBranchRegionStaff(short id, UserInfo user);

        bool UpdateCollectionRetailComputationVariables(CollectionsRetailComputationVariableSetupViewModel model, short id);

        bool ValidateRegionName(string regionName);

        bool UpdateBranches(BranchViewModel model, short id);

        IEnumerable<BranchViewModel> GetSearchedBranch(string query);
        // tbl_Branch Limit
        //BranchLimitViewModel GetBranchLimit(short id);

        //IEnumerable<BranchLimitViewModel> GetAllBranchLimit();

        //IEnumerable<BranchLimitViewModel> GetAllBranchLimitByBranchId(int id);

        //Task<bool> AddBranchLimit(BranchLimitViewModel model);

        //Task<bool> UpdateBranchLimit(BranchLimitViewModel model);

        //Task<bool> DeleteBranchLimit(short id);
        Task<bool> AddRetailCollectionComputationVariableAsync(CollectionsRetailComputationVariableSetupViewModel model);
    }
}