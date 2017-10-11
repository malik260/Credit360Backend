using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    
    public class PublicHolidayRepository : IPublicHolidayRepository
    {
        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository genSetup;
        private FinTrakBankingContext context;
        public PublicHolidayRepository(FinTrakBankingContext _context,
                                IAuditTrailRepository _auditTrail,
                                IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
        }

        public PublicHolidayViewModel GetPublicHoliday(int id)
        {
            var holiday = context.tbl_Public_Holiday.Find(id);

            if (holiday != null)
            {
                return new PublicHolidayViewModel()
                {
                    PublicHolidayId = holiday.PublicHolidayId,
                    CountryId = holiday.CountryId,
                    Date = holiday.Date,
                    CountryName = context.tbl_Country.FirstOrDefault(x => x.CountryId == holiday.CountryId).Name ?? string.Empty,
                    Description = holiday.Description
                };
            }

            return new PublicHolidayViewModel();
        }

        public IEnumerable<PublicHolidayViewModel> GetAllPublicHoliday()
        {
            var holidays = context.tbl_Public_Holiday.Select(x => new PublicHolidayViewModel
            {
                PublicHolidayId = x.PublicHolidayId,
                CountryId = x.CountryId,
                Date = x.Date,
                CountryName = context.tbl_Country.FirstOrDefault(k => k.CountryId == x.CountryId).Name ?? string.Empty,
                Description = x.Description
            }).ToList();

            return holidays;
        }

        public IEnumerable<PublicHolidayViewModel> GetAllPublicHolidayByCompanyId(int id)
        {
            var holidays = (from a in context.tbl_Public_Holiday
                            join b in context.tbl_Company
                            on a.CountryId equals b.CountryId
                            where b.CompanyId == id
                            select new PublicHolidayViewModel
                            {
                                PublicHolidayId = a.PublicHolidayId,
                                CountryId = a.CountryId,
                                Date = a.Date,
                                CountryName = b.Name ?? string.Empty,
                                Description = a.Description
                            });

            return holidays;
        }
        public bool isHolidayExist(DateTime description)
        {
            return context.tbl_Public_Holiday.Any(x => x.Date == description.Date);
        }
        public bool AddPublicHoliday(PublicHolidayViewModel model)
        {
            var holiday = new tbl_Public_Holiday()
            {
                CountryId = model.CountryId,
                Date = model.Date,
                Description = model.Description
            };

            this.context.tbl_Public_Holiday.Add(holiday);
            ///---Audit Section ---------------------------
           var audit = new tbl_Audit
           {
               AuditTypeId = (short)AuditTypeEnum.PublicHolidayAdded,
               StaffId = (int)model.createdBy,
               BranchId = (short)model.userBranchId,
               Detail = $"Added Holiday: '{model.Description}' with Id: {model.PublicHolidayId} ",
               IPAddress = model.userIPAddress,
               Url = model.applicationUrl,
               ApplicationDate = genSetup.GetApplicationDate(),
               SystemDateTime = DateTime.Now
           };
            this.auditTrail.AddAuditTrail(audit);
            ///-----end of Audit section -------------------------------
            var response = context.SaveChanges();
            return response != 0;
        }

        public bool UpdatePublicHoliday(PublicHolidayViewModel model, int id)
        {
            var response = 0;
            var holiday = context.tbl_Public_Holiday.Find(id);

            if (holiday != null)
            {
                holiday.Date = model.Date;
                holiday.Description = model.Description;
                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.PublicHolidayUpdated,
                    StaffId = (int)model.createdBy,
                    BranchId = (short)model.userBranchId,
                    Detail = $"Updated branch: '{model.Description}' with CountryId: {model.CountryId} ",
                    IPAddress = model.userIPAddress,
                    Url = model.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };
                this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------
                response = context.SaveChanges();
            }

            return response != 0;
        }

        //public async Task<bool> DeleteBranch(int id, UserInfo user)
        //{
        //    var response = 0;
        //    var holiday = context.tbl_Public_Holiday.Remove(id);

        //    if (holiday != null)
        //    {
        //        response = await context.SaveChangesAsync();
        //        // Audit Section ---------------------------
        //        var audit = new tbl_Audit
        //        {
        //            AuditTypeId = (short)AuditTypeEnum.BranchDeleted,
        //            StaffId = (int)user.staffId,
        //            BranchId = (short)user.BranchId,
        //            Detail = $"Deleted branch: '{branch.BranchName}' with code: {branch.BranchCode} ",
        //            IPAddress = user.userIPAddress,
        //            Url = user.applicationUrl,
        //            ApplicationDate = genSetup.GetApplicationDate(),
        //            SystemDateTime = DateTime.Now
        //        };
        //        //end of Audit section -------------------------------
        //    }

        //    return response != 0;
        //}
    }
}
