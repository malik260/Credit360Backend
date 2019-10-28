using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public class StaffModifiedInformationReport
    {
        public List<StaffPrivilegeChangeViewModel> StaffPrivilegeChange(DateTime startDate, DateTime endDate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                IQueryable<StaffPrivilegeChangeViewModel> data = (from a in context.TBL_TEMP_STAFF
                                                       join b in context.TBL_STAFF_ROLE on a.STAFFROLEID equals b.STAFFROLEID
                                                       join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                       join d in context.TBL_STAFF on a.STAFFCODE equals d.STAFFCODE
                                                       join e in context.TBL_STAFF_ROLE on d.STAFFROLEID equals e.STAFFROLEID

                                                       where a.COMPANYID == companyId && a.APPROVALSTATUSID == (short)(ApprovalStatusEnum.Approved)
                                                       && (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                                       && DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                       && a.STAFFROLEID != d.STAFFROLEID
                                                       && d.COMPANYID == companyId
                                                                  orderby a.DATETIMECREATED descending
                                                                  select new StaffPrivilegeChangeViewModel()
                                                       {


                                                           staffFullName = a.FIRSTNAME + " " + " " + a.MIDDLENAME + " "+ " " + a.LASTNAME,
                                                           
                                                           staffCreatedByName = c.FIRSTNAME + "" +" " + c.LASTNAME +" " + "-" + c.STAFFCODE,
                                                           previousStaffRoleName = b.STAFFROLENAME,
                                                           currentStaffRoleName = e.STAFFROLENAME,
                                                           dateTimeCreated = a.DATETIMECREATED.Value,
                                                           tempStaffCode = a.STAFFCODE
                                                       });


                return data.ToList();
            }

        }
            public List<UserGroupProfileViewModel> UserGroupChangeReport(DateTime startDate, DateTime endDate, int companyId)
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    IQueryable<UserGroupProfileViewModel> data = (from a in context.TBL_TEMP_PROFILE_USERGROUP
                                                                  join b in context.TBL_PROFILE_GROUP on a.GROUPID equals b.GROUPID
                                                                  join c in context.TBL_PROFILE_USER on a.TEMPUSERID equals c.USERID
                                                                  //join d in context.TBL_TEMP_STAFF on c.TEMPSTAFFID equals d.TEMPSTAFFID
                                                                  join e in context.TBL_STAFF on a.CREATEDBY equals e.STAFFID
                                                                  
                                                                  where e.COMPANYID  == companyId && a.APPROVALSTATUSID == (short)(ApprovalStatusEnum.Approved)
                                                                  //&& (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                                                  //&& DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                                   
                                                                  orderby a.DATETIMECREATED descending
                                                                  select new UserGroupProfileViewModel()
                                                                  {
                                                                        username = c.USERNAME,
                                                                        groupName = b.GROUPNAME,
                                                                        createdBy = e.FIRSTNAME + "" + e.MIDDLENAME + "" + e.LASTNAME,
                                                                        dateTimeCreated = a.DATETIMECREATED,
                                                                        //dateTimeUpdated = (a.DATETIMEUPDATED.Value == null? default(DateTime) : a.DATETIMEUPDATED.Value)
                                                                    });

                    var output = data.ToList();

                    return output;
                }

            }


        public List<ProfileActivityReportViewModel> ProfileActivityReport(DateTime startDate, DateTime endDate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                IQueryable<ProfileActivityReportViewModel> data = (from a in context.TBL_TEMP_PROFILE_ADTN_ACTIVITY
                                                                   join b in context.TBL_PROFILE_ACTIVITY on a.ACTIVITYID equals b.ACTIVITYID
                                                                   join c in context.TBL_PROFILE_USER on a.TEMPUSERID equals c.USERID
                                                                   join e in context.TBL_STAFF on a.CREATEDBY equals e.STAFFID





                                                                   where e.COMPANYID == companyId && a.CANAPPROVE == true
                                                              && (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                                              && DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                                   orderby a.DATETIMECREATED descending

                                                                   select new ProfileActivityReportViewModel()
                                                                   {
                                                                       activityName = b.ACTIVITYNAME,
                                                                       createdBy = e.FIRSTNAME + "" + e.MIDDLENAME + "" + e.LASTNAME,
                                                                       dateTimeCreated = a.DATETIMECREATED,
                                                                       //dateTimeUpdated = (a.DATETIMEUPDATED.Value == null ? default(DateTime) : a.DATETIMEUPDATED.Value)


                                                                   });


                return data.ToList();
            }

        }
        public List<StaffRoleProfileGroupReportViewModel> StaffRoleProfileGroupReport(DateTime startDate, DateTime endDate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                IQueryable<StaffRoleProfileGroupReportViewModel> data = (from a in context.TBL_TEMP_PROFILE_STAFF_ROL_GRP
                                                                         join b in context.TBL_STAFF_ROLE on a.STAFFROLEID equals b.STAFFROLEID
                                                                         join c in context.TBL_PROFILE_GROUP on a.GROUPID equals c.GROUPID
                                                                         join e in context.TBL_STAFF on a.CREATEDBY equals e.STAFFID





                                                                         where e.COMPANYID == companyId && a.APPROVALSTATUSID == (short)(ApprovalStatusEnum.Approved)
                                                                    && (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                                                    && DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))

                                                                         orderby a.DATETIMECREATED descending

                                                                         select new StaffRoleProfileGroupReportViewModel()
                                                                         {
                                                                             groupName = c.GROUPNAME,
                                                                             staffRoleName = b.STAFFROLENAME,
                                                                             createdBy = e.FIRSTNAME + "" + e.MIDDLENAME + "" + e.LASTNAME,
                                                                             dateTimeCreated = a.DATETIMECREATED,
                                                                             //dateTimeUpdated = (a.DATETIMEUPDATED.Value == null ? default(DateTime) : a.DATETIMEUPDATED.Value)


                                                                         });


                return data.ToList();
            }

        }
            public List<StaffRoleProfileActivityReportViewModel> StaffRoleProfileActivityReport(DateTime startDate, DateTime endDate, int companyId)
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    IQueryable<StaffRoleProfileActivityReportViewModel> data = (from a in context.TBL_TEMP_PROFILE_STAFF_ROLE_AA
                                                                       join b in context.TBL_PROFILE_ACTIVITY on a.ACTIVITYID equals b.ACTIVITYID
                                                                       join c in context.TBL_STAFF_ROLE on a.STAFFROLEID equals c.STAFFROLEID
                                                                       join e in context.TBL_STAFF on a.CREATEDBY equals e.STAFFID





                                                                       where e.COMPANYID == companyId && a.APPROVALSTATUSID == (short)(ApprovalStatusEnum.Approved)
                                                                  && (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                                                  && DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))

                                                                                orderby a.DATETIMECREATED descending

                                                                                select new StaffRoleProfileActivityReportViewModel()
                                                                       {
                                                                           activityName = b.ACTIVITYNAME,
                                                                           staffRoleName = c.STAFFROLENAME,
                                                                           createdBy = e.FIRSTNAME + "" + e.MIDDLENAME + "" + e.LASTNAME,
                                                                           dateTimeCreated = a.DATETIMECREATED,
                                                                           //dateTimeUpdated = (a.DATETIMEUPDATED.Value == null ? default(DateTime) : a.DATETIMEUPDATED.Value)


                                                                       });


                    return data.ToList();
                }

            }


        }
    }


