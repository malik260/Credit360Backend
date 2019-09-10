using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
   public class RSR
    {
        FinTrakBankingContext context = new FinTrakBankingContext();
       
        public IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReports(int projectSiteReportId)
        {

            return context.TBL_PSR_PROJECT_SITE_REPORT.Where(x => x.PROJECTSITEREPORTID == projectSiteReportId).Select(x => new ProjectSiteReportViewModel
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
                currencyId = x.CURRENCYID,
                approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),

            }).OrderByDescending(o => o.projectSiteReportId)
                .ToList();
        }

        public IEnumerable<PsrPerformanceEvaluationViewModel> GetProjectSiteReportsApg(int id)
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
                    apgReceived = x.AMOUNTRECEIVED,
                    psrReportTypeId = x.PSRREPORTTYPEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o => o.PSRREPORTTYPEID == x.PROJECTSITEREPORTID).Select(o => o.REPORTTYPENAME).FirstOrDefault(),
                }).OrderBy(o => o.psrPerformanceEvaluationId)
                .ToList();
        }

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
                    psrReportType = context.TBL_PSR_REPORT_TYPE.Where(o => o.PSRREPORTTYPEID == x.PROJECTSITEREPORTID).Select(o => o.REPORTTYPENAME).FirstOrDefault(),
                }).OrderBy(o => o.psrPerformanceEvaluationId)
                .ToList();
        }
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

        public IEnumerable<PsrRecommendationViewModel> Getrecomendations(int id)
        {

            return context.TBL_PSR_RECOMMENDATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrRecommendationViewModel
                {
                    psrRecommendationId = x.PSRRECOMMENDATIONID,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    projectRiskRating = x.PROJECTRISKRATING,
                    customerRating = x.CUSTOMERRATING,
                    comment = x.COMMENTS,
                })
                .ToList();
        }

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

        public IEnumerable<LoanApplicationViewModel> GetFacilities(int id)
        {
            return (from p in context.TBL_PSR_PROJECT_FACILITIES
                    join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                    // join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                    let loan_Application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()

                    where p.PROJECTSITEREPORTID == id

                    select new LoanApplicationViewModel
                    {
                        // customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        // customerCode = c.CUSTOMERCODE,
                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        //  customerId = c.CUSTOMERID,
                        //branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        applicationDate = x.APPLICATIONDATE,
                        applicationAmount = x.APPLICATIONAMOUNT,
                        interestRate = x.INTERESTRATE,
                        productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == loan_Application_detail.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                        tenor = loan_Application_detail.APPROVEDTENOR,
                        relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        relationshipManagerId = x.RELATIONSHIPMANAGERID,
                        relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        operationId = (int)OperationsEnum.OriginalDocumentApproval,
                        isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                    }).ToList();
        }
    }
}
