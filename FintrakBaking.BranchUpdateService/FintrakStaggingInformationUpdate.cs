
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
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
            foreach (var staging in stagingStaff)
            {

                var frantrakStaff = (from b in coreContext.TBL_STAFF
                                     where b.STAFFCODE == staging.STAFFCODE
                                     select new { b }).FirstOrDefault();

                if (frantrakStaff == null)
                {
                    var model = new TBL_STAFF
                    {
                        ADDRESS = staging.ADRESS2,
                        BRANCHID = coreContext.TBL_BRANCH.FirstOrDefault(o => o.BRANCHCODE == staging.BRANCHCODE).BRANCHID,
                        FIRSTNAME = staging.FIRSTNAME,
                        MIDDLENAME = staging.MIDDLENAME,
                        LASTNAME = staging.LASTNAME,
                        PHONE = staging.PHONE,
                        EMAIL = staging.EMAIL,
                        DEPARTMENTID = coreContext.TBL_DEPARTMENT.FirstOrDefault(o => o.DEPARTMENTCODE == staging.DEPARTMENTCODE.ToString()).DEPARTMENTID,
                        STAFFCODE = staging.STAFFCODE,
                        SUPERVISOR_STAFFID = coreContext.TBL_STAFF.FirstOrDefault(s => s.STAFFCODE == staging.SUPERVISORSTAFFCODE).STAFFID,
                        COMPANYID = 1,
                        JOBTITLEID = 2,
                        STAFFROLEID = 3,
                        NPL_LIMITEXCEEDED = false,
                        DELETED = false,
                        CUSTOMERSENSITIVITYLEVELID = 3

                    };

                    coreContext.TBL_STAFF.Add(model);


                    response = coreContext.SaveChanges();
                    if (response != 0)
                    {
                        StaffAdded = StaffAdded + ", " + staging.STAFFCODE;
                    }

                }
                else
                {
                    // update branch
                    string newBranchCode = stagingContext.STG_STAFF.FirstOrDefault(o => o.STAFFCODE == staging.STAFFCODE).BRANCHCODE;

                    var br = coreContext.TBL_BRANCH.Where(o => o.BRANCHCODE == newBranchCode).FirstOrDefault();

                    if (br != null)
                    {

                        var sStaff = stagingStaff.FirstOrDefault(s => s.STAFFCODE == staging.STAFFCODE);
                        var supervisorStaffId = coreContext.TBL_STAFF.FirstOrDefault(s => s.STAFFCODE == staging.SUPERVISORSTAFFCODE);
                        var deptID = coreContext.TBL_DEPARTMENT.FirstOrDefault(o => o.DEPARTMENTCODE == staging.DEPARTMENTCODE.ToString());

                        TBL_STAFF val = coreContext.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE == staging.STAFFCODE);
                        val.BRANCHID = br.BRANCHID;
                        if (val.SUPERVISOR_STAFFID!=null)
                        {
                        val.SUPERVISOR_STAFFID = supervisorStaffId.STAFFID;

                        }

                        response = coreContext.SaveChanges();

                        if (response != 0)
                        {
                            StaffAdded = StaffAdded + ", " + staging.STAFFCODE;
                        }


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
