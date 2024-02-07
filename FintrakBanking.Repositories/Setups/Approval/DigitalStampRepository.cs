using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class DigitalStampRepository: IDigitalStampRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;



        public DigitalStampRepository(
            FinTrakBankingContext _context,
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;

        }

        public bool AddDigitalStamp(DigitalStampViewModel model)
        {
            var existing = context.TBL_DIGITAL_STAMP.Where(x => x.DELETED == false
                       && x.STAFFROLEID == model.staffRoleId
                       && x.STAFFROLEID == model.staffRoleId);
            if (existing.Any()) 
            { 
                this.UpdateDigitalStamp(model.digitalStampId, model); 
                return true;
            }
            else
            {
                model.datetimeCreated = DateTime.Now;

                var stamp = new TBL_DIGITAL_STAMP
                {
                    STAFFROLEID = model.staffRoleId,
                    STAMPNAME = model.stampName,
                    DIGITALSTAMP = model.digitalStamp,
                    DELETED = false,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = model.datetimeCreated,
                   
                };
                context.TBL_DIGITAL_STAMP.Add(stamp);
            }
           

            if(context.SaveChanges()> 0 )
            {
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.DigitalStampUpload,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Digital Stamp setup uploaded successfully by staff with STAFFID {model.createdBy}",
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
                return true;
            }
            return false;

        }

        public bool DeleteDigitalStamp(int digitalStampid, UserInfo user)
        {
            var digitalStamp = context.TBL_DIGITAL_STAMP.Find(digitalStampid);
            if(digitalStamp != null)
            {
                digitalStamp.DELETED = true;
                digitalStamp.DELETEDBY = user.createdBy;
                digitalStamp.DATETIMEDELETED = DateTime.Now;

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Digital stamp deleted was deleted by {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = digitalStamp.STAFFROLEID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }
            if (context.SaveChanges() > 0) return true;
            return false;
        }

        public IEnumerable<DigitalStampViewModel> GetAllDigitalStamp()
        {
            var stamps = (from x in context.TBL_DIGITAL_STAMP
                          where x.DELETED == false
                          select new DigitalStampViewModel
                          {
                              digitalStampId = x.DIGITALSTAMPID,
                              staffRoleId = x.STAFFROLEID,
                              staffRoleName = context.TBL_STAFF_ROLE.Where(r =>r.STAFFROLEID == x.STAFFROLEID).FirstOrDefault().STAFFROLENAME,
                              stampName = x.STAMPNAME,
                              digitalStamp = x.DIGITALSTAMP,
                              deleted = x.DELETED,
                              deletedBy = x.DELETEDBY,
                              datetimeDeleted = x.DATETIMEDELETED,
                              createdBy = x.CREATEDBY,
                              datetimeCreated = x.DATETIMECREATED,
                              updatedBy = x.UPDATEDBY,
                              datetimeUpdated = x.DATETIMEUPDATED,
                             
                              
                          }).ToList();

            return stamps;
        }

        public DigitalStampViewModel GetDigitalStampByApprovalLevel(int staffRoleId)
        {
            var stamps = (from x in context.TBL_DIGITAL_STAMP
                          where x.DELETED == false && x.STAFFROLEID == staffRoleId
                          select new DigitalStampViewModel
                          {
                              digitalStampId = x.DIGITALSTAMPID,
                              staffRoleId = x.STAFFROLEID,
                              stampName = x.STAMPNAME,
                              digitalStamp = x.DIGITALSTAMP,
                              deleted = x.DELETED,
                              deletedBy = x.DELETEDBY,
                              datetimeDeleted = x.DATETIMEDELETED,
                              createdBy = x.CREATEDBY,
                              datetimeCreated = x.DATETIMECREATED,
                              updatedBy = x.UPDATEDBY,
                              datetimeUpdated = x.DATETIMEUPDATED,
                              
                          }).FirstOrDefault();

            return stamps;
        }

        public bool UpdateDigitalStamp(int digitalStampid, DigitalStampViewModel model)
        {
            var digitalStamp = context.TBL_DIGITAL_STAMP.Find(digitalStampid);
            if (digitalStamp == null)
            {
                digitalStamp = context.TBL_DIGITAL_STAMP.Where(s=> s.STAFFROLEID == model.staffRoleId).FirstOrDefault();
            }
            if (digitalStamp != null)
            {
                digitalStamp.DIGITALSTAMP = model.digitalStamp;
                digitalStamp.UPDATEDBY = model.createdBy;
                digitalStamp.DATETIMEUPDATED = DateTime.Now;
                

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Digital stamp deleted was deleted by {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = digitalStamp.STAFFROLEID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }
            if (context.SaveChanges() > 0) return true;
            return false;
        }
    }
}
