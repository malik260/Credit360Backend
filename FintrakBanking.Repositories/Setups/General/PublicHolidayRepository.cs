using FintrakBanking.Common;
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

            var holidays = (from x in context.TBL_PUBLIC_HOLIDAY
                            join k in context.TBL_COUNTRY on x.COUNTRYID equals k.COUNTRYID
                            where x.PUBLICHOLIDAYID == id
                            select new PublicHolidayViewModel
                            {
                                publicHolidayId = x.PUBLICHOLIDAYID,
                                countryId = x.COUNTRYID,
                                date = x.DATE,
                                countryName = k.NAME ?? string.Empty,
                                description = x.DESCRIPTION,
                                isActive = x.ISACTIVE
                            });
           

            return holidays.FirstOrDefault();
        }


        public IEnumerable<PublicHolidayViewModel> GetAllPublicHoliday()
        {
            var holidays = (from x in context.TBL_PUBLIC_HOLIDAY
                            join k in context.TBL_COUNTRY on x.COUNTRYID equals k.COUNTRYID
                            select new PublicHolidayViewModel
                            {
                                publicHolidayId = x.PUBLICHOLIDAYID,
                                countryId = x.COUNTRYID,
                                date = x.DATE,
                                countryName = k.NAME ?? string.Empty,
                                description = x.DESCRIPTION,
                                isActive = x.ISACTIVE
                            }).ToList();

            return holidays;
        }

        public IEnumerable<PublicHolidayViewModel> GetAllPublicHolidayByCompanyId(int id)
        {
            var holidays = (from a in context.TBL_PUBLIC_HOLIDAY
                            join b in context.TBL_COMPANY
                            on a.COUNTRYID equals b.COUNTRYID
                            where b.COMPANYID == id
                            select new PublicHolidayViewModel
                            {
                                publicHolidayId = a.PUBLICHOLIDAYID,
                                countryId = a.COUNTRYID,
                                date = a.DATE,
                                countryName = b.NAME ?? string.Empty,
                                description = a.DESCRIPTION,
                                isActive = a.ISACTIVE
                            });

            return holidays;
        }

        public bool DoesHolidayExist(DateTime date, int countryId)
        {
            var output = context.TBL_PUBLIC_HOLIDAY.Any(x => x.DATE == date.Date);
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
            List<TBL_PUBLIC_HOLIDAY> datesToAdd =  new List<TBL_PUBLIC_HOLIDAY>();
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
                        datesToAdd.Add(new TBL_PUBLIC_HOLIDAY { COUNTRYID = model.countryId, DATE = currentDate, ISACTIVE = true, DESCRIPTION = weekEndInfo });
                    }
                    else
                    {
                        datesToUpdate.Add(currentDate);
                    }
                }

            }

            if(datesToAdd.Count > 0)
               this.context.TBL_PUBLIC_HOLIDAY.AddRange(datesToAdd);

            if(datesToUpdate.Count > 0)
            {
                var result = from a in context.TBL_PUBLIC_HOLIDAY
                             where a.COUNTRYID == model.countryId && datesToUpdate.Contains(a.DATE)
                             select a;

                foreach (var item in result)
                {
                    item.ISACTIVE = true;
                    item.DESCRIPTION = weekEndInfo;
                }
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PublicHolidayAdded,
                STAFFID = (int)model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added all weekends in the year : '{model.date.Year}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            var response = context.SaveChanges();

            return response != 0;
        }


        public bool AddPublicHoliday(PublicHolidayViewModel model)
        {
            var holiday = new TBL_PUBLIC_HOLIDAY()
            {
                COUNTRYID = model.countryId,
                DATE = model.date,
                DESCRIPTION = model.description,
                ISACTIVE = true
            };

            this.context.TBL_PUBLIC_HOLIDAY.Add(holiday);
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PublicHolidayAdded,
                STAFFID = (int)model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Holiday: '{model.description}' with Id: {model.publicHolidayId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            var response = context.SaveChanges();
            return response != 0;
        }

        public bool UpdatePublicHoliday(PublicHolidayViewModel model, int id)
        {
            var response = 0;
            var holiday = context.TBL_PUBLIC_HOLIDAY.Find(id);

            if (holiday != null)
            {
                holiday.DATE = model.date;
                holiday.DESCRIPTION = model.description;
                holiday.ISACTIVE = model.isActive;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.PublicHolidayUpdated,
                    STAFFID = (int)model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated branch: '{model.description}' with CountryId: {model.countryId} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------
                response = context.SaveChanges();
            }

            return response != 0;
        }

        public bool DeletePublicHoliday(UserInfo model, int id)
        {
            var response = 0;
            var holiday = context.TBL_PUBLIC_HOLIDAY.Where(o=>o.PUBLICHOLIDAYID==id).FirstOrDefault();

            if (holiday != null)
            {

                context.TBL_PUBLIC_HOLIDAY.Remove(holiday);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.PublicHolidayUpdated,
                    STAFFID = (int)model.staffId,
                    BRANCHID = (short)model.BranchId,
                    DETAIL = $"Delete Public holyday with public Holiday Id: " + id,
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
        //    var holiday = context.TBL_PUBLIC_HOLIDAY.Remove(id);

        //    if (holiday != null)
        //    {
        //        response = await context.SaveChangesAsync();
        //        // Audit Section ---------------------------
        //        var audit = new TBL_AUDIT
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
