using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.Repositories.Admin
{
    public class AuditTrailRepository : IAuditTrailRepository
    {
        private FinTrakBankingContext context;

        public AuditTrailRepository(FinTrakBankingContext _contex)
        {
            this.context = _contex;
        }

        public void AddAuditTrail(List<TBL_AUDIT> auditInput)
        {
            context.TBL_AUDIT.AddRange(auditInput);
        }

        public void AddAuditTrail(TBL_AUDIT auditInput)
        {
            context.TBL_AUDIT.Add(auditInput);
        }

        public IQueryable<AuditViewModel> GetAuditTrail(short branchId)
        {
            return from _audit in context.TBL_AUDIT
                   join atype in context.TBL_AUDIT_TYPE on _audit.AUDITTYPEID equals atype.AUDITTYPEID
                   join st in context.TBL_STAFF on _audit.STAFFID equals st.STAFFID
                   join u in context.TBL_PROFILE_USER on st.STAFFID equals u.STAFFID
                   join b in context.TBL_BRANCH on _audit.BRANCHID equals b.BRANCHID
                   where _audit.BRANCHID == branchId
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
                       auditTypeId = _audit.AUDITTYPEID,
                   };
        }
        public IQueryable<DeletedStaffLog> GetDeletedStaffLog(short branchId)
        {
            return from _audit in context.TBL_STAFF
                   where _audit.DELETED == true
                   //join atype in context.TBL_AUDIT_TYPE on _audit.AUDITTYPEID equals atype.AUDITTYPEID
                   //join st in context.TBL_STAFF on _audit.STAFFID equals st.STAFFID
                   //join u in context.TBL_PROFILE_USER on st.STAFFID equals u.STAFFID
                   //join b in context.TBL_BRANCH on _audit.BRANCHID equals b.BRANCHID
                   //where _audit.BRANCHID == branchId
                   select new DeletedStaffLog
                   {
                       deletedById = _audit.DELETEDBY,
                       deletedByName = context.TBL_STAFF.Where(a=>a.STAFFID == _audit.DELETEDBY).Select(q=>q.FIRSTNAME + " " + q.MIDDLENAME + " " + q.LASTNAME).FirstOrDefault(),// _audit.APPLICATIONDATE,
                       deletedStaffId = _audit.STAFFID,
                       deletedStaffName = _audit.FIRSTNAME + " " + _audit.MIDDLENAME + " " + _audit.LASTNAME,
                       deletedDate = _audit.DATETIMEDELETED,
                       deletedStaffCode = _audit.STAFFCODE,
                       deletedByStaffCode = context.TBL_STAFF.Where(a => a.STAFFID == _audit.DELETEDBY).Select(q => q.STAFFCODE).FirstOrDefault(),// _audit.APPLICATIONDATE,

                   };
        }

        public IQueryable<DormantStaffLog> GetDormantStaffLog(short branchId)
        {
            var currentDateTime = DateTime.Now.Date;
            context.TBL_PROFILE_SETTING.AsNoTracking();

            int userInactivePeriod = context.TBL_PROFILE_SETTING.FirstOrDefault().MAXPERIODOFUSERINACTIVITY;


            var inactiveUsersSub = (from a in context.TBL_PROFILE_USER
                                    where a.ISACTIVE == true //&& a.LASTLOGINDATE != null
                                    //&& (DbFunctions.AddDays(a.LASTLOGINDATE, userInactivePeriod) <= DbFunctions.AddDays(currentDateTime, 0))
                                    && (DbFunctions.DiffDays(a.LASTLOGINDATE, currentDateTime) > userInactivePeriod || a.LASTLOGINDATE == null)
                                    select new DormantStaffLog {
                                       userId = a.STAFFID,
                                       staffCode= context.TBL_STAFF.Where(x => x.STAFFID == a.STAFFID).Select(p => p.STAFFCODE).FirstOrDefault(),
                                        staffName = context.TBL_STAFF.Where(x=>x.STAFFID == a.STAFFID).Select(p=>p.FIRSTNAME + " " + p.MIDDLENAME +p.LASTNAME).FirstOrDefault(),
                                       lastLoginDate = a.LASTLOGINDATE,
                                    });


            return inactiveUsersSub;
        }

    }
}