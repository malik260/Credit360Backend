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

            var branches = context.TBL_BRANCH.Where(x => x.DELETED == false).Select(x => new BranchViewModel
            {
                branchId = x.BRANCHID,
                stateId = x.STATEID,
                cityId = (int)x.CITYID,
                companyId = x.COMPANYID,
                stateName = x.TBL_STATE.STATENAME,
                cityName = context.TBL_CITY.FirstOrDefault(c => c.CITYID == x.CITYID).CITYNAME ?? string.Empty,
                branchName = x.BRANCHNAME,
                branchCode = x.BRANCHCODE,
                addressLine1 = x.ADDRESSLINE1,
                addressLine2 = x.ADDRESSLINE2,
                comment = x.COMMENT_,
                deleted = x.DELETED,
            }).ToList();

            return branches; ;

        }

        public static BranchViewModel GetBranch(short id)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var branch = context.TBL_BRANCH.Find(id);

            if (branch != null)
            {
                return new BranchViewModel()
                {
                    branchId = branch.BRANCHID,
                    stateId = branch.STATEID,
                    companyId = branch.COMPANYID,
                    branchName = branch.BRANCHNAME,
                    branchCode = branch.BRANCHCODE,
                    addressLine1 = branch.ADDRESSLINE1,
                    addressLine2 = branch.ADDRESSLINE2,
                    comment = branch.COMMENT_,
                };
            }

            return new BranchViewModel();
        }
    }
}
