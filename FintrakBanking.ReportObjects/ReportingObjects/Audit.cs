using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
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
                where (_audit.SYSTEMDATETIME >= startDate && _audit.SYSTEMDATETIME <= endDate)
                && (st.FIRSTNAME.StartsWith(username.Trim()) || st.MIDDLENAME.StartsWith(username.Trim())
                || st.LASTNAME.StartsWith(username.Trim()) || u.USERNAME.StartsWith(username.Trim())
                || _audit.URL.StartsWith(username.Trim())
                || _audit.BRANCHID == context.TBL_BRANCH.Where(x => x.BRANCHCODE == username).Select(x => x.BRANCHID).FirstOrDefault()
                || atype.AUDITTYPENAME.ToLower().StartsWith(username.ToLower().Trim())
                || _audit.DETAIL.ToLower().Contains(username.ToLower().Trim())
                || username == null)
                &&(_audit.AUDITTYPEID== auditTypeId || auditTypeId==0)
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

            };
                return data.ToList();
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
