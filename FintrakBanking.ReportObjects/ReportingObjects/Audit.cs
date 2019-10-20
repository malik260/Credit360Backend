using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
   public class Audit
    {
        public IEnumerable<AuditViewModel> GetAuditTrailByParam(DateTime startDate, DateTime endDate, string username, int auditTypeId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data =  from _audit in context.TBL_AUDIT
                join atype in context.TBL_AUDIT_TYPE on _audit.AUDITTYPEID equals atype.AUDITTYPEID
                join st in context.TBL_STAFF on _audit.STAFFID equals st.STAFFID
                join u in context.TBL_PROFILE_USER on st.STAFFID equals u.STAFFID
                join b in context.TBL_BRANCH on _audit.BRANCHID equals b.BRANCHID
                where (DbFunctions.TruncateTime(_audit.SYSTEMDATETIME) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(_audit.SYSTEMDATETIME) <= DbFunctions.TruncateTime(endDate))
                && (st.FIRSTNAME.StartsWith(username.Trim()) || st.MIDDLENAME.StartsWith(username.Trim())
                || st.LASTNAME.StartsWith(username.Trim()) || u.USERNAME.StartsWith(username.Trim())
                || _audit.URL.StartsWith(username.Trim())
                || _audit.BRANCHID == context.TBL_BRANCH.Where(x => x.BRANCHCODE == username).Select(x => x.BRANCHID).FirstOrDefault()
                || atype.AUDITTYPENAME.ToLower().StartsWith(username.ToLower().Trim())
                || _audit.DETAIL.ToLower().Contains(username.ToLower().Trim())
                || username == null || username == String.Empty)
                && (_audit.AUDITTYPEID == (short)auditTypeId || auditTypeId==0)
                orderby _audit.SYSTEMDATETIME descending
                select new AuditViewModel
                {
                    auditId = _audit.AUDITID,
                    applicationDate = _audit.APPLICATIONDATE,
                    auditType = atype.AUDITTYPENAME,
                    details = _audit.DETAIL,
                    firstName = st.FIRSTNAME,
                    lastName = st.LASTNAME,
                    systemDate = _audit.SYSTEMDATETIME,
                    username = u.USERNAME,
                    url = _audit.URL,
                    branchName = b.BRANCHNAME,
                    ipAddress = _audit.IPADDRESS,
                    deviceName = _audit.DEVICENAME,
                    osName = _audit.OSNAME

            };
                return data.ToList();
            }
        
        }


        public IEnumerable<LoggingActivities> GetLoggingStatus(DateTime startDate, DateTime endDate, bool logingStatus,  string branchCode)
        {
            DateTime defaultDate = new DateTime(2018, 1, 1);

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from lg in context.TBL_PROFILE_USER
                           join st in context.TBL_STAFF on lg.USERNAME equals st.STAFFCODE
                           join b in context.TBL_BRANCH on st.BRANCHID equals b.BRANCHID
                           where (DbFunctions.TruncateTime(lg.LASTLOGINDATE) >= DbFunctions.TruncateTime(startDate)
                           && DbFunctions.TruncateTime(lg.LASTLOGINDATE) <= DbFunctions.TruncateTime(endDate))
                           && (lg.ISACTIVE == logingStatus)
                           //where (DbFunctions.TruncateTime((lg.LASTLOGINDATE.Value == null ? defaultDate : lg.LASTLOGINDATE.Value)) >= DbFunctions.TruncateTime(startDate)
                           //&& DbFunctions.TruncateTime((lg.LASTLOGINDATE.Value == null ? defaultDate : lg.LASTLOGINDATE.Value)) <= DbFunctions.TruncateTime(endDate))
                           //&& (lg.ISACTIVE == logingStatus) //&& (b.BRANCHCODE==branchCode || branchCode=="")
                           orderby lg.LASTLOGINDATE descending
                           select new LoggingActivities
                           {
                              approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID== lg.APPROVALSTATUSID).Select(o=>o.APPROVALSTATUSNAME).FirstOrDefault(),
                              branchName = b.BRANCHNAME,
                              dateCreated = lg.DATETIMECREATED,
                              deactivatedDate = lg.DEACTIVATEDDATE,
                              failedLoggingAttempts = lg.FAILEDLOGONATTEMPT,
                              isUserActive = lg.ISACTIVE,
                              isUserLocked = lg.ISLOCKED,
                              lastLogginDate = lg.LASTLOGINDATE,
                              lastLogOutDate = lg.LASTLOCKOUTDATE,
                              names = st.FIRSTNAME + " " + st.LASTNAME + " " + st.MIDDLENAME,
                              userName = lg.USERNAME,
                              branchCode = b.BRANCHCODE
                           };
                var result = data.ToList();
                return result;
            }

        }
        public IEnumerable<AuditViewModel> AuditType(string searchValue)
        {


            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
               return context.TBL_AUDIT_TYPE.Where(x=>x.AUDITTYPENAME.Contains(searchValue.ToUpper())).Select(x => new AuditViewModel
                {
                    auditTypeId = x.AUDITTYPEID,
                    auditType = x.AUDITTYPENAME
                });

            }

        }
    }
}
