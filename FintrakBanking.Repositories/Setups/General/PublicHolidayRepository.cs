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
    [Export(typeof(IPublicHolidayRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
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
                    publicHolidayId = holiday.PublicHolidayId,
                    countryId = holiday.CountryId,
                    date = holiday.Date,
                    countryName = context.tbl_Country.FirstOrDefault(x => x.CountryId == holiday.CountryId).Name ?? string.Empty,
                    description = holiday.Description,
                    isActive = holiday.IsActive
                };
            }

            return new PublicHolidayViewModel();
        }


        public IEnumerable<PublicHolidayViewModel> GetAllPublicHoliday()
        {
            var holidays = context.tbl_Public_Holiday.Select(x => new PublicHolidayViewModel
            {
                publicHolidayId = x.PublicHolidayId,
                countryId = x.CountryId,
                date = x.Date,
                countryName = context.tbl_Country.FirstOrDefault(k => k.CountryId == x.CountryId).Name ?? string.Empty,
                description = x.Description,
                isActive = x.IsActive
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
                                publicHolidayId = a.PublicHolidayId,
                                countryId = a.CountryId,
                                date = a.Date,
                                countryName = b.Name ?? string.Empty,
                                description = a.Description,
                                isActive = a.IsActive
                            });

            return holidays;
        }

        public bool DoesHolidayExist(DateTime date, int countryId)
        {
            var output = context.tbl_Public_Holiday.Any(x => x.Date == date.Date);
            return output;
        }

        public DateTime GetNextWorkDay(DateTime date, int countryId)
        {
            var nextWorkDay = date.AddDays(1);

            while (DoesHolidayExist(nextWorkDay, countryId) == true)
            {
                nextWorkDay = nextWorkDay.AddDays(1);
            }

            return nextWorkDay;
        }

        public bool AddWeekendsInTheYear(PublicHolidayViewModel model)
        {
            List<tbl_Public_Holiday> datesToAdd =  new List<tbl_Public_Holiday>();
            List<DateTime> datesToUpdate = new List<DateTime>();


            DateTime startDate = new DateTime(model.date.Year, 1, 1);
            DateTime endDate = new DateTime(model.date.Year, 12, 31);
            var weekEndInfo = "weekend date";

            TimeSpan diff = endDate - startDate;
            int days = diff.Days;
            for (var i = 0; i <= days; i++)
            {
                var currentDate = startDate.AddDays(i);
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (DoesHolidayExist(currentDate, model.countryId) == false)
                    {
                        datesToAdd.Add(new tbl_Public_Holiday { CountryId = model.countryId, Date = currentDate, IsActive = true, Description = weekEndInfo });
                    }
                    else
                    {
                        datesToUpdate.Add(currentDate);
                    }
                }

            }

            if(datesToAdd.Count > 0)
               this.context.tbl_Public_Holiday.AddRange(datesToAdd);

            if(datesToUpdate.Count > 0)
            {
                var result = from a in context.tbl_Public_Holiday
                             where a.CountryId == model.countryId && datesToUpdate.Contains(a.Date)
                             select a;

                foreach (var item in result)
                {
                    item.IsActive = true;
                    item.Description = weekEndInfo;
                }
            }

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.PublicHolidayAdded,
                StaffId = (int)model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added all weekends in the year : '{model.date.Year}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            var response = context.SaveChanges();

            return response != 0;
        }


        public bool AddPublicHoliday(PublicHolidayViewModel model)
        {
            var holiday = new tbl_Public_Holiday()
            {
                CountryId = model.countryId,
                Date = model.date,
                Description = model.description,
                IsActive = true
            };

            this.context.tbl_Public_Holiday.Add(holiday);
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.PublicHolidayAdded,
                StaffId = (int)model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Holiday: '{model.description}' with Id: {model.publicHolidayId} ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            var response = context.SaveChanges();
            return response != 0;
        }

        public bool UpdatePublicHoliday(PublicHolidayViewModel model, int id)
        {
            var response = 0;
            var holiday = context.tbl_Public_Holiday.Find(id);

            if (holiday != null)
            {
                holiday.Date = model.date;
                holiday.Description = model.description;
                holiday.IsActive = model.isActive;

                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.PublicHolidayUpdated,
                    StaffId = (int)model.createdBy,
                    BranchId = (short)model.userBranchId,
                    Detail = $"Updated branch: '{model.description}' with CountryId: {model.countryId} ",
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
