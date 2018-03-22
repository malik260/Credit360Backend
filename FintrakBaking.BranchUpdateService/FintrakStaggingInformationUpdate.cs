using FintrakBaking.BranchUpdateService.Fintrak_Model;
using FintrakBaking.BranchUpdateService.Staging_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBaking.BranchUpdateService
{
    public class FintrakStaggingInformationUpdate
    {
        FinTrakBankingContext coreContext = new FinTrakBankingContext();
        FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext();

        public string AddNewBranches()
        {
            var response = 0;
            string AddedBranch = string.Empty;
            var stagingBranches = stagingContext.STG_BRANCH;
            foreach (var x in stagingBranches)
            {
                var frantrakBranck = coreContext.TBL_BRANCH.Where(o => x.BRANCHCODE == x.BRANCHCODE).FirstOrDefault();
                if (frantrakBranck == null)
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
                        REGIONID = 1

                    };

                    coreContext.TBL_BRANCH.Add(model);

                    try
                    {
                        response = coreContext.SaveChanges();
                        if (response != 0)
                        {
                            AddedBranch = AddedBranch + ", " + x.BRANCHNAME;
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

        public string UpdateStaffInformation()
        {
            var response = 0;
            string StaffAdded = string.Empty;
            string StaffUpdated = string.Empty;
            var stagingStaff = stagingContext.STG_STAFF;
            foreach (var x in stagingStaff)
            {
                var frantrakStaff = coreContext.TBL_STAFF.Where(o => x.STAFFCODE == x.STAFFCODE).FirstOrDefault();
                if (frantrakStaff == null)
                {

                    var model = new TBL_STAFF
                    {
                        ADDRESS = x.ADDRESS1,
                        BRANCHID = coreContext.TBL_BRANCH.FirstOrDefault(o => o.BRANCHCODE == x.BRANCHCODE).BRANCHID,
                        FIRSTNAME = x.FIRSTNAME,
                        MIDDLENAME = x.MIDDLENAME,
                        LASTNAME = x.LASTNAME,
                        PHONE = x.PHONE,
                        EMAIL = x.EMAIL,
                        DEPARTMENTID = coreContext.TBL_DEPARTMENT.FirstOrDefault(o => o.DEPARTMENTCODE == x.DEPARTMENTCODE.ToString()).DEPARTMENTID,
                        STAFFCODE = x.STAFFCODE,
                        SUPERVISOR_STAFFID = coreContext.TBL_STAFF.FirstOrDefault(s => s.STAFFCODE == stagingContext.STG_STAFF.FirstOrDefault(o => o.STAFFCODE == x.STAFFCODE).SUPERVISORSTAFFCODE).STAFFID,
                    };

                    coreContext.TBL_STAFF.Add(model);

                    try
                    {
                        response = coreContext.SaveChanges();
                        if (response != 0)
                        {
                            StaffAdded = StaffAdded + ", " + x.STAFFCODE;
                        }
                    }
                    catch (Exception ex) { }
                }
                else
                {
                    // update branch
                    string newBranchCode = stagingContext.STG_STAFF.FirstOrDefault(o => o.STAFFCODE == x.STAFFCODE).BRANCHCODE;

                    TBL_STAFF branch = new TBL_STAFF();

                    var br = coreContext.TBL_BRANCH.FirstOrDefault(o => o.BRANCHCODE == newBranchCode);
                   
                    if (br.BRANCHID != null)
                    {
                        try
                        {
                            branch.BRANCHID = br.BRANCHID;
                            response = coreContext.SaveChanges();
                            if (response != 0)
                            {
                                StaffUpdated = StaffUpdated + ", " + x.STAFFCODE;
                            }
                        }
                        catch (Exception ex) { }
                    }
                }
            }
            if (response == 1)
            {
                return "The following new staff added on " + DateTime.Now.ToString() + " : " + StaffAdded + " | " + StaffUpdated + " Updated";
            }
            return "No branch added";
        }

    }
}
