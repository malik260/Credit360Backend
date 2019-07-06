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
                    where x.ISPROJECTRELATED == true && (x.APPLICATIONREFERENCENUMBER == searchString
                 || c.FIRSTNAME.ToLower().Contains(searchString.Trim())
                 || c.LASTNAME.ToLower().Contains(searchString.Trim())
                 || c.MIDDLENAME.ToLower().Contains(searchString.Trim()))
                 

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
                        operationId = (int)OperationsEnum.OriginalDocumentApproval,
                        isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                    }).ToList();
        }

        public IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReports(int id)
        {
            return context.TBL_PSR_PROJECT_SITE_REPORT.Where(x => x.DELETED == false && x.LOANAPPLICATIONID == id)
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
                    approvalStatusId = x.APPROVALSTATUSID,
                    approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),

                }).OrderBy(o=>o.projectSiteReportId)
                .ToList();
        }

        public bool SubmitApproval(ProjectSiteReportViewModel model)
        {
            bool responce = false;

            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.projectSiteReportId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.ProjectSiteReportApproval;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var document = context.TBL_PSR_PROJECT_SITE_REPORT.Where(o => o.PROJECTSITEREPORTID == model.projectSiteReportId).FirstOrDefault();
                        if (document != null)
                        {
                            document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        }

                    }

                    responce = context.SaveChanges() > 0;
                    transaction.Commit();

                    return responce;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }
        }

        public IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReportApprovals(int staffId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ProjectSiteReportApproval).ToList();

            return (from x in context.TBL_PSR_PROJECT_SITE_REPORT
                    join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                    join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                   join atrail in context.TBL_APPROVAL_TRAIL on x.PROJECTSITEREPORTID equals atrail.TARGETID
                   where x.DELETED == false && atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                    && atrail.RESPONSESTAFFID == null
                    && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                    && atrail.OPERATIONID == (int)OperationsEnum.ProjectSiteReportApproval
                   select new ProjectSiteReportViewModel
                   {
                        appplicationReferenceNumber = l.APPLICATIONREFERENCENUMBER,
                        customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
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
                       approvalStatusId = x.APPROVALSTATUSID,
                       operationId = atrail.OPERATIONID,

                       approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),

                   }).OrderBy(o => o.projectSiteReportId)
                .ToList();
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

          var id =  context.TBL_PSR_PROJECT_SITE_REPORT.Add(entity);

            if (context.SaveChanges() > 0)
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = id.PROJECTSITEREPORTID;
                workflow.Comment = "Request for Project Site Report approval";
                workflow.OperationId = (int)OperationsEnum.ProjectSiteReportApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

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
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    comment = x.COMMENTS,
                })
                .ToList();
        }


        public bool AddPsrRecommendation(PsrRecommendationViewModel model)
        {
            var entity = new TBL_PSR_RECOMMENDATION
            {
                COMMENTS = model.comment,
                PROJECTSITEREPORTID = model.projectSiteReportId,
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
                    apgIssued = x.APGISSUED,
                    disbursedTodate = x.DISBURSEDTODATE,
                    initialProjectSum = x.INITIALPROJECTSUM,
                    paymentToDate = x.PAYMENTTODATE,
                    pmuAssessed = x.PMUASSESSED,
                    projectSum = x.PROJECTSUM,
                    progressPayment = x.PROGRESSPAYMENT,
                    vowdToDate = x.VOWDTODATE,
                    amortisedApg = x.AMORTISEDAPG,
                    costVariation = x.COSTVARIATION,
                    certifiedVowd = x.CERTIFIEDVOWD,
                    timeVariation = x.TIMEVARIATION,
                    consoltantVowd = x.CONSULTANTVOWD,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    amountReceived = x.AMOUNTRECEIVED,
                    psrReportTypeId = x.PSRREPORTTYPEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o=>o.PSRREPORTTYPEID== x.PROJECTSITEREPORTID).Select(o=>o.REPORTTYPENAME).FirstOrDefault(),
                }).OrderBy(o => o.psrPerformanceEvaluationId)
                .ToList();
        }
        public bool UpdatePsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model, int id)
        {
            var entity = this.context.TBL_PSR_PERFORMANCE_EVALUATION.Find(id);
            entity.APGISSUED = model.apgIssued;
            entity.DISBURSEDTODATE = model.disbursedTodate;
            entity.INITIALPROJECTSUM = model.initialProjectSum;
            entity.PAYMENTTODATE = model.paymentToDate;
            entity.PMUASSESSED = model.pmuAssessed;
            entity.PROJECTSUM = model.projectSum;
            entity.VOWDTODATE = model.vowdToDate;
            entity.COSTVARIATION = model.costVariation;
            entity.TIMEVARIATION = model.timeVariation;
            entity.CONSULTANTVOWD = model.consoltantVowd;
            entity.PROGRESSPAYMENT = model.progressPayment;
            entity.CREATEDBY = model.createdBy;
            entity.AMORTISEDAPG = model.amortisedApg;
            entity.AMOUNTRECEIVED = model.amountReceived;
            entity.CERTIFIEDVOWD = model.certifiedVowd;

            entity.LASTUPDATEDBY = model.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.PsrPerformanceEvaluationUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"TBL_Psr Performance Evaluation '{entity.PSRPERFORMANCEEVALUATIONID}' was updated by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PSRPERFORMANCEEVALUATIONID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddPsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model)
        {
            var entity = new TBL_PSR_PERFORMANCE_EVALUATION
            {
                APGISSUED = model.apgIssued,
                DISBURSEDTODATE = model.disbursedTodate,
                INITIALPROJECTSUM = model.initialProjectSum,
                PAYMENTTODATE = model.paymentToDate,
                PMUASSESSED = model.pmuAssessed,
                PROJECTSUM = model.projectSum,
                VOWDTODATE = model.vowdToDate,
                COSTVARIATION = model.costVariation,
                TIMEVARIATION = model.timeVariation,
                CONSULTANTVOWD = model.consoltantVowd,
                PROGRESSPAYMENT = model.progressPayment,
                CREATEDBY = model.createdBy,
                AMORTISEDAPG = model.amortisedApg,
                AMOUNTRECEIVED = model.amountReceived,
                CERTIFIEDVOWD = model.certifiedVowd,
                DATETIMECREATED = general.GetApplicationDate(),
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                PROJECTSITEREPORTID = model.projectSiteReportId,
                PSRREPORTTYPEID = model.psrReportTypeId
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
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    comment = x.COMMENTS,
                })
                .ToList();
        }

        public bool AddPsrObservation(PsrObservationViewModel model)
        {
            var entity = new TBL_PSR_OBSERVATION
            {
                COMMENTS = model.comment,
                PROJECTSITEREPORTID = model.projectSiteReportId,
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

        public IEnumerable<PsrNextInspectionTaskViewModel> GetPsrNextInspectionTasks(int id)
        {
            return context.TBL_PSR_NEXT_INSPECTION_TASK.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrNextInspectionTaskViewModel
                {
                    psrNextInspectionTaskId = x.PSRNEXTINSPECTIONTASKID,
                    comment = x.COMMENTS,
                    isDone = x.ISDONE,
                    nextInspectionDate = x.NEXTINSPECTIONDATE,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                })
                .ToList();
        }

        public bool AddPsrNextInspectionTask(PsrNextInspectionTaskViewModel model)
        {
            var entity = new TBL_PSR_NEXT_INSPECTION_TASK
            {
                COMMENTS = model.comment,
                ISDONE = model.isDone,
                PROJECTSITEREPORTID = model.projectSiteReportId,
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
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    comment = x.COMMENTS,
                })
                .ToList();
        }

        public bool AddPsrComment(PsrCommentViewModel model)
        {
            var entity = new TBL_PSR_COMMENT
            {
                COMMENTS = model.comment,
                PROJECTSITEREPORTID = model.projectSiteReportId,
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
