using FintrakBaking.BranchUpdateService.Fintrak_Model;
using FintrakBaking.BranchUpdateService.Staging_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBaking.BranchUpdateService
{
    public class GetUpdatedNewBranches
    {
        FinTrakBankingContext coreContext = new FinTrakBankingContext();
        FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext();

      public string AddNewBranches()
        {
            var response = 0;
            string AddedBranch = string.Empty;
           var stagingBranches = stagingContext.STG_BRANCH;
            foreach(var x in stagingBranches)
            {
                var frantrakBranck =  coreContext.TBL_BRANCH.Where(o => x.BRANCHCODE == x.BRANCHCODE).FirstOrDefault();
                if (frantrakBranck==null)
                {
                    var stateId = coreContext.TBL_STATE.Where(a => a.STATECODE == x.STATECODE).FirstOrDefault().STATEID;
                    var cityId = coreContext.TBL_CITY.Where(a => a.STATEID == stateId).FirstOrDefault().CITYID;

                    var model = new TBL_BRANCH
                    {
                        BRANCHCODE = x.BRANCHCODE,
                        BRANCHNAME = x.BRANCHNAME,
                        ADDRESSLINE1 = x.ADDRESSLINE1,
                        ADDRESSLINE2 = x.ADDRESSLINE2,
                        STATEID = stateId,
                        CITYID = cityId,
                        DELETED = false,
                        DATETIMECREATED = DateTime.Now,
                        REGIONID=1
                        
                        };
                 
                    coreContext.TBL_BRANCH.Add(model);

                    try
                    {
                      response =  coreContext.SaveChanges();
                        if (response !=0)
                        {
                            AddedBranch  = AddedBranch + ", " + x.BRANCHNAME;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            if (!String.IsNullOrEmpty(AddedBranch))
            {
                return "The following new branch added on " + DateTime.Now.ToString() + " : " + AddedBranch;
            }
            return "No branch added";
        }
    }
}
