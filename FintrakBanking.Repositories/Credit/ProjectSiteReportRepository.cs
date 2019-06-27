using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Repositories.credit
{
    public class ProjectSiteReportRepository : IProjectSiteReportRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public ProjectSiteReportRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
        }
        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            return (from x in context.TBL_LOAN_APPLICATION
                    join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                    join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                    where x.APPLICATIONREFERENCENUMBER == searchString
                 || c.FIRSTNAME.ToLower().Contains(searchString.Trim())
                 || c.LASTNAME.ToLower().Contains(searchString.Trim())
                 || c.MIDDLENAME.ToLower().Contains(searchString.Trim())

                    select new LoanApplicationViewModel
                    {
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        customerId = c.CUSTOMERID,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        applicationDate = x.APPLICATIONDATE,
                        applicationAmount = x.APPLICATIONAMOUNT,
                        interestRate = x.INTERESTRATE,
                        productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                        relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        relationshipManagerId = x.RELATIONSHIPMANAGERID,
                        relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        operationId = (int)OperationsEnum.OriginalDocumentApproval
                    }).ToList();
        }

        public IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReports()
        {
            return context.TBL_PSR_PROJECT_SITE_REPORT.Where(x => x.DELETED == false)
                .Select(x => new ProjectSiteReportViewModel
                {
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    psrReportTypeId = x.PSRREPORTTYPEID,
                    clientName = x.CLIENTNAME,
                    contractorName = x.CONTRACTORNAME,
                    consultantName = x.CONSULTANTNAME,
                    projectAmount = x.PROJECTAMOUNT,
                    projectDescription = x.PROJECTDESCRIPTION,
                    commencementDate = x.COMMENCEMENTDATE,
                    completionDate = x.COMPLETIONDATE,
                    nextVisitationDate = x.NEXTVISITATIONDATE,
                    loanApplicationId = x.LOANAPPLICATIONID,
                    projectLocation = x.PROJECTLOCATION,
                    approvalStatusId = x.APPROVALSTATUSID
                })
                .ToList();
        }

        public ProjectSiteReportViewModel GetProjectSiteReport(int id)
        {
            var entity = context.TBL_PSR_PROJECT_SITE_REPORT.FirstOrDefault(x => x.LOANAPPLICATIONID == id && x.DELETED == false);

            return new ProjectSiteReportViewModel
            {
                projectSiteReportId = entity.PROJECTSITEREPORTID,
                psrReportTypeId = entity.PSRREPORTTYPEID,
                clientName = entity.CLIENTNAME,
                contractorName = entity.CONTRACTORNAME,
                consultantName = entity.CONSULTANTNAME,
                projectAmount = entity.PROJECTAMOUNT,
                projectDescription = entity.PROJECTDESCRIPTION,
                commencementDate = entity.COMMENCEMENTDATE,
                completionDate = entity.COMPLETIONDATE,
                nextVisitationDate = entity.NEXTVISITATIONDATE,
                loanApplicationId = entity.LOANAPPLICATIONID,
                projectLocation = entity.PROJECTLOCATION,
                approvalStatusId = entity.APPROVALSTATUSID,
            };
        }

        public bool AddProjectSiteReport(ProjectSiteReportViewModel model)
        {
            var entity = new TBL_PSR_PROJECT_SITE_REPORT
            {
                PSRREPORTTYPEID = model.psrReportTypeId,
                CLIENTNAME = model.clientName,
                CONTRACTORNAME = model.contractorName,
                CONSULTANTNAME = model.consultantName,
                PROJECTAMOUNT = model.projectAmount,
                PROJECTDESCRIPTION = model.projectDescription,
                COMMENCEMENTDATE = model.commencementDate,
                COMPLETIONDATE = model.completionDate,
                NEXTVISITATIONDATE = model.nextVisitationDate,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                LOANAPPLICATIONID = model.loanApplicationId,
                PROJECTLOCATION = model.projectLocation,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing
            };

            context.TBL_PSR_PROJECT_SITE_REPORT.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProjectSiteReportAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_PSR_Project Site Report '{entity.CLIENTNAME}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateProjectSiteReport(ProjectSiteReportViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            entity.PSRREPORTTYPEID = model.psrReportTypeId;
            entity.CLIENTNAME = model.clientName;
            entity.CONTRACTORNAME = model.contractorName;
            entity.CONSULTANTNAME = model.consultantName;
            entity.PROJECTAMOUNT = model.projectAmount;
            entity.PROJECTDESCRIPTION = model.projectDescription;
            entity.COMMENCEMENTDATE = model.commencementDate;
            entity.COMPLETIONDATE = model.completionDate;
            entity.NEXTVISITATIONDATE = model.nextVisitationDate;
            entity.PROJECTLOCATION = model.projectLocation;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProjectSiteReportUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_PSR_Project Site Report '{entity.CLIENTNAME}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PROJECTSITEREPORTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteProjectSiteReport(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProjectSiteReportDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_PSR_Project Site Report '{entity.CLIENTNAME}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PROJECTSITEREPORTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        public IEnumerable<PsrReportTypeViewModel> GetPsrReportTypes()
        {
            return context.TBL_PSR_REPORT_TYPE.Where(x => x.DELETED == false)
                .Select(x => new PsrReportTypeViewModel
                {
                    psrReportTypeId = x.PSRREPORTTYPEID,
                    reportTypeName = x.REPORTTYPENAME,
                })
                .ToList();
        }
    }
}

           // kernel.Bind<IProjectSiteReportRepository>().To<ProjectSiteReportRepository>();
           // ProjectSiteReportAdded = ???, ProjectSiteReportUpdated = ???, ProjectSiteReportDeleted = ???,
