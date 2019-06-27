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

        #region
        public IEnumerable<PsrRecommendationViewModel> GetPsrRecommendations(int id)
        {
            return context.TBL_PSR_RECOMMENDATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrRecommendationViewModel
                {
                    psrRecommendationId = x.PSRRECOMMENDATIONID,
                    comment = x.COMMENT,
                })
                .ToList();
        }

        
        public bool AddPsrRecommendation(PsrRecommendationViewModel model)
        {
            var entity = new TBL_PSR_RECOMMENDATION
            {
                COMMENT = model.comment,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_RECOMMENDATION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrRecommendationAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Recommendation '{entity.PSRRECOMMENDATIONID}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrRecommendation(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_RECOMMENDATION.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrRecommendationDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Recommendation '{entity.PSRRECOMMENDATIONID}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRRECOMMENDATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region

        public IEnumerable<PsrPerformanceEvaluationViewModel> GetPsrPerformanceEvaluations(int id)
        {
            return context.TBL_PSR_PERFORMANCE_EVALUATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrPerformanceEvaluationViewModel
                {
                    psrPerformanceEvaluationId = x.PSRPERFORMANCEEVALUATIONID,
                    grossAmount = x.GROSSAMOUNT,
                    amountReceived = x.AMOUNTRECEIVED,
                    progressPayment = x.PROGRESSPAYMENT,
                    certifiedValueWorkDone = x.CERTIFIEDVALUEWORKDONE,
                    managementUnitValueWorkDone = x.MANAGEMENTUNITVALUEWORKDONE,
                    consultantValueWorkDone = x.CONSULTANTVALUEWORKDONE,
                    costVariation = x.COSTVARIATION,
                    timeVariation = x.TIMEVARIATION,
                })
                .ToList();
        }

        public bool AddPsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model)
        {
            var entity = new TBL_PSR_PERFORMANCE_EVALUATION
            {
                GROSSAMOUNT = model.grossAmount,
                AMOUNTRECEIVED = model.amountReceived,
                PROGRESSPAYMENT = model.progressPayment,
                CERTIFIEDVALUEWORKDONE = model.certifiedValueWorkDone,
                MANAGEMENTUNITVALUEWORKDONE = model.managementUnitValueWorkDone,
                CONSULTANTVALUEWORKDONE = model.consultantValueWorkDone,
                COSTVARIATION = model.costVariation,
                TIMEVARIATION = model.timeVariation,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_PERFORMANCE_EVALUATION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceEvaluationAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Performance Evaluation '{entity.PSRPERFORMANCEEVALUATIONID}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrPerformanceEvaluation(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_PERFORMANCE_EVALUATION.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceEvaluationDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Performance Evaluation '{entity.PSRPERFORMANCEEVALUATIONID}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRPERFORMANCEEVALUATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion


        #region
        public IEnumerable<PsrObservationViewModel> GetPsrObservations(int id)
        {
            return context.TBL_PSR_OBSERVATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrObservationViewModel
                {
                    psrObservationId = x.PSROBSERVATIONID,
                    comment = x.COMMENT,
                })
                .ToList();
        }

        public bool AddPsrObservation(PsrObservationViewModel model)
        {
            var entity = new TBL_PSR_OBSERVATION
            {
                COMMENT = model.comment,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_OBSERVATION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrObservationAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Observation '{entity.PSROBSERVATIONID}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrObservation(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_OBSERVATION.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrObservationDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                //DETAIL = $"TBL_Psr Observation '{entity.DESCRIPTION}' was deleted by {auditStaff}",
                DETAIL = $"TBL_Psr Observation was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSROBSERVATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion


        #region

        public IEnumerable<PsrNextInspectionTaskViewModel> GetPsrNextInspectionTasks( int id)
        {
            return context.TBL_PSR_NEXT_INSPECTION_TASK.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrNextInspectionTaskViewModel
                {
                    psrNextInspectionTaskId = x.PSRNEXTINSPECTIONTASKID,
                    comment = x.COMMENT,
                    isDone = x.ISDONE,
                    nextInspectionDate =  x.NEXTINSPECTIONDATE
                })
                .ToList();
        }
        
        public bool AddPsrNextInspectionTask(PsrNextInspectionTaskViewModel model)
        {
            var entity = new TBL_PSR_NEXT_INSPECTION_TASK
            {
                COMMENT = model.comment,
                ISDONE = model.isDone,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                NEXTINSPECTIONDATE = model.nextInspectionDate,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_NEXT_INSPECTION_TASK.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrNextInspectionTaskAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Next Inspection Task '{entity.PSRNEXTINSPECTIONTASKID}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrNextInspectionTask(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_NEXT_INSPECTION_TASK.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrNextInspectionTaskDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Next Inspection Task '{entity.PSRNEXTINSPECTIONTASKID}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRNEXTINSPECTIONTASKID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        #endregion

        #region
        public IEnumerable<PsrCommentViewModel> GetPsrComments(int id)
        {
            return context.TBL_PSR_COMMENT.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrCommentViewModel
                {
                    psrCommentId = x.PSRCOMMENTID,
                    comment = x.COMMENT,
                })
                .ToList();
        }

        public bool AddPsrComment(PsrCommentViewModel model)
        {
            var entity = new TBL_PSR_COMMENT
            {
                COMMENT = model.comment,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_PSR_COMMENT.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrCommentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Psr Comment '{entity.PSRCOMMENTID}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeletePsrComment(int id, UserInfo user)
        {
            var entity = this.context.TBL_PSR_COMMENT.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrCommentDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Psr Comment '{entity.PSRCOMMENTID}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRCOMMENTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        #endregion
    }
}

           // kernel.Bind<IProjectSiteReportRepository>().To<ProjectSiteReportRepository>();
           // ProjectSiteReportAdded = ???, ProjectSiteReportUpdated = ???, ProjectSiteReportDeleted = ???,
