
using FintrakBaking.DataMigration;
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

            var branchCode = coreContext.TBL_BRANCH.Select(o => o.BRANCHCODE).ToList();
            var newBranch = (from f in stagingContext.STG_BRANCH
                             where !branchCode.Contains(f.BRANCHCODE)
                             select f).ToList();

           foreach(var f in newBranch)
            {
                TBL_BRANCH x = new TBL_BRANCH();
                x.BRANCHCODE = f.BRANCHCODE;
                x.BRANCHNAME = f.BRANCHNAME;
                x.ADDRESSLINE1 = f.ADDRESSLINE1;
                x.ADDRESSLINE2 = f.ADDRESSLINE2;
                x.STATEID = coreContext.TBL_STATE.Where(a => a.STATECODE == f.STATECODE).Select(a => a.STATEID).FirstOrDefault();
                x.CITYID = coreContext.TBL_CITY.Where(a => a.TBL_LOCALGOVERNMENT.STATEID == coreContext.TBL_STATE.Where(g => g.STATECODE == f.STATECODE).Select(g => g.STATEID).FirstOrDefault()).Select(a => a.CITYID).FirstOrDefault();
                x.DELETED = false;
                x.DATETIMECREATED = DateTime.Now;
                x.REGIONID = 1;
                AddedBranch = AddedBranch + f.BRANCHNAME + ";";
                coreContext.TBL_BRANCH.Add(x);
            }

            if (coreContext.SaveChanges()>0)
            {
                return "The following new branch added on " + DateTime.Now.ToString() + " : " + AddedBranch.TrimEnd(';');
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
                        STAFFCODE = staging.STAFFCODE,
                        SUPERVISOR_STAFFID = coreContext.TBL_STAFF.FirstOrDefault(s => s.STAFFCODE == staging.SUPERVISORSTAFFCODE).STAFFID,
                        COMPANYID = 1,
                        JOBTITLEID = 2,
                        STAFFID = 3,
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
