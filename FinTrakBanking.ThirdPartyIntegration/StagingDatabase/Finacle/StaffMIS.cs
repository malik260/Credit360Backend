using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinTrakBanking.ThirdPartyIntegration.StagingDatabase.Finacle
{
    public class StaffMIS  : IStaffMIS 
    {
        FinTrakBankingStagingContext stagingContext;
        FinTrakBankingContext context;
        public StaffMIS(FinTrakBankingContext _context, FinTrakBankingStagingContext _stagingContext)
        {
            context = _context;
            stagingContext = _stagingContext;
        }
       public StaffAccountHistoryViewModel StaffInformationSystem(int staffId)
        {
            var staffCode = context.TBL_STAFF.Where(x => x.STAFFID == staffId).Select(x=>x.STAFFCODE).FirstOrDefault();
            if (staffCode!=null)
            {
                return (from staff in stagingContext.STG_STAFFMIS
                        where staff.USERNAME == staffCode
                        select new StaffAccountHistoryViewModel
                        {
                            field1 = staff.USERNAME,
                            field2 = staff.TEAM_UNIT,
                            field3 = staff.COST_CENT,
                            field4 = staff.DEPT_NAME,
                            field5 = staff.REGION,
                            field6 = staff.GROUP_HUB,
                            field7 = staff.DIRECTORATE

                        }).FirstOrDefault();
            }
            throw new SecureException("Staff does not exist in the staff MIS record");

        }
    }
}
