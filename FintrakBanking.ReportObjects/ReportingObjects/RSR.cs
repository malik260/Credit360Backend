using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            var project = context.TBL_PSR_PROJECT_SITE_REPORT.Find(projectSiteReportId);
            var currency = context.TBL_CURRENCY.Find(project.CURRENCYID);
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
                //nextVisitationDate = x.NEXTVISITATIONDATE,
                loanApplicationId = x.LOANAPPLICATIONID,
                projectLocation = x.PROJECTLOCATION,
                approvalStatusId = x.APPROVALSTATUSID,
                inspectionDate = x.INSPECTIONDATE,
                currencyId = x.CURRENCYID,
                currency = currency.CURRENCYCODE,
                approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                //currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),

            }).OrderByDescending(o => o.projectSiteReportId)
                .ToList();
        }

        public IEnumerable<PsrPerformanceEvaluationViewModel> GetProjectSiteReportsApg(int id)
        {
            var project = context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            var currency = context.TBL_CURRENCY.Find(project.CURRENCYID);

            return context.TBL_PSR_PERFORMANCE_EVALUATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrPerformanceEvaluationViewModel
                {
                    psrPerformanceEvaluationId = x.PSRPERFORMANCEEVALUATIONID,
                    apgIssued = x.APGISSUED,
                    percentageOne = (x.APGISSUED / x.PROJECTSUM) * 100,
                    percentageTwo = (x.AMOUNTRECEIVED / x.PROJECTSUM) * 100,
                    percentageThree = (x.AMORTISEDAPG / x.APGISSUED) * 100,
                    percentageFour = (x.CERTIFIEDVOWD / x.PROJECTSUM) * 100,
                    percentageFive = ((x.PMUASSESSED / x.PROJECTSUM) * 100) == null ? 0 : ((x.PMUASSESSED / x.PROJECTSUM) * 100),
                    currency = currency.CURRENCYCODE,
                    disbursedTodate = x.DISBURSEDTODATE,
                    initialProjectSum = x.INITIALPROJECTSUM,
                    paymentToDate = x.PAYMENTTODATE,
                    pmuAssessed = x.PMUASSESSED == null ? 0 : x.PMUASSESSED,
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
            var projectSite = context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            var loanApprovedAmount = context.TBL_LOAN_APPLICATION.Find(projectSite.LOANAPPLICATIONID);
            var currency = context.TBL_CURRENCY.Find(projectSite.CURRENCYID);

            return context.TBL_PSR_PERFORMANCE_EVALUATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrPerformanceEvaluationViewModel
                {
                    amountDisbursedPercent = (x.DISBURSEDTODATE/x.PROJECTSUM)*100,
                    pmuPercentage = (x.PMUASSESSED / x.PROJECTSUM) * 100,
                    psrPerformanceEvaluationId = x.PSRPERFORMANCEEVALUATIONID,
                    apgIssued = x.APGISSUED,
                    currency = currency.CURRENCYCODE,
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

        public IEnumerable<PsrPerformanceAnalysisViewModel> GetPsrPerformanceAnalysis(int id)
        {
            var project = context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            var currency = context.TBL_CURRENCY.Find(project.CURRENCYID);

            return context.TBL_PSR_ANALYSIS.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrPerformanceAnalysisViewModel
                {
                    psrAnalysisId = x.PSRANALYSISID,
                    ipc = (x.IPC != null && x.IPC > 0) ? x.IPC : x.PMU,
                    pmu = x.PMU,
                    currency = currency.CURRENCYCODE,
                    aTotal = (x.IPC != null && x.IPC > 0) ? (x.IPC+x.VALUEOFCOLLATERAL) : (x.PMU+x.VALUEOFCOLLATERAL),
                    bTotal = (x.AMOUNTDISBURSED+x.AMOUNTREQUESTED),
                    netPerformance = (x.IPC != null && x.IPC > 0) ? ((x.IPC + x.VALUEOFCOLLATERAL) - (x.AMOUNTDISBURSED + x.AMOUNTREQUESTED)) : ((x.PMU + x.VALUEOFCOLLATERAL) - (x.AMOUNTDISBURSED + x.AMOUNTREQUESTED)),
                    amountDisbursed = x.AMOUNTDISBURSED,
                    whatToShow = (x.IPC != null && x.IPC > 0) ? "Certified VOWD to Date" : "PMU Assessed VOWD to Date",
                    amountRequested = x.AMOUNTREQUESTED,
                    valueOfCollateral = x.VALUEOFCOLLATERAL,
                    projectSiteReportId = x.PROJECTSITEREPORTID
                }).ToList(); 
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
            var psr = context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            var loanApplication = context.TBL_LOAN_APPLICATION.Find(psr.LOANAPPLICATIONID);

            //var facilityRating = "";
            //var customerRating = "";
            //if (loanApplication.CUSTOMERID != null)
            //{
            //    var customer = context.TBL_CUSTOMER.Find(loanApplication.CUSTOMERID);
            //    customerRating = customer.CUSTOMERRATING;
            //    facilityRating = context.TBL_FACILITY_RATING.Where(c => c.CUSTOMERCODE == customer.CUSTOMERCODE).Select(c => c.PROBABILITYOFDEFAULT).FirstOrDefault();
            //}
            //else
            //{
            //    var customer = context.TBL_CUSTOMER_GROUP.Find(loanApplication.CUSTOMERGROUPID);
            //    var rating = context.TBL_CUSTOMER_RISK_RATING.Find(customer.RISKRATINGID);
            //    customerRating = rating.RISKRATING; 
            //    facilityRating = context.TBL_FACILITY_RATING.Where(c => c.CUSTOMERCODE == customer.GROUPCODE).Select(c => c.PROBABILITYOFDEFAULT).FirstOrDefault();

            //}
          var records = context.TBL_PSR_RECOMMENDATION.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                        .Select(x => new PsrRecommendationViewModel
                        {
                            psrRecommendationId = x.PSRRECOMMENDATIONID,
                            projectSiteReportId = x.PROJECTSITEREPORTID,
                            //projectRiskRating = facilityRating,
                            //customerRating = customerRating,
                            comment = x.COMMENTS,
                        })
                        .ToList();
            var data = records.GroupBy(r => r.psrRecommendationId)
                               .Select(r => r.FirstOrDefault()).ToList();
            foreach (var rec in data)
            {
                var contractorTiering = (from a in context.TBL_CONTRACTOR_TIERING
                                         where a.LOANAPPLICATIONID == psr.LOANAPPLICATIONID && a.CUSTOMERID == loanApplication.CUSTOMERID
                                         select new
                                         {
                                             contractorTierId = a.CONTRACTORTIERID,
                                             loanApplicationId = a.LOANAPPLICATIONID,
                                             customerId = a.CUSTOMERID,
                                             actualValue = a.ACTUALVALUE
                                         }).AsEnumerable().Select(a => new ContractorTieringViewModel
                                         {
                                             contractorTierId = a.contractorTierId,
                                             loanApplicationId = a.loanApplicationId,
                                             customerId = a.customerId,
                                             actualValue = a.actualValue
                                         }).ToList();

                var result = contractorTiering.Select(a => new ContractorTieringViewModel
                {
                    contractorTierId = a.contractorTierId,
                    loanApplicationId = a.loanApplicationId,
                    customerId = a.customerId,
                    criteria = a.criteria,
                    actualValue = a.actualValue,
                    computation = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == a.loanApplicationId).Sum(d => d.ACTUALVALUE),
                }).ToList();

                foreach (var check in result)
                {
                    if (check.computation >= 80)
                    {
                        rec.customerRating = "Tier 1";
                    }
                    if (check.computation >= 60 && check.computation <= 79)
                    {
                        rec.customerRating = "Tier 2";
                    }
                    if (check.computation <= 59)
                    {
                        rec.customerRating = "Tier 3";
                    }
                }

                var customerTier1 = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == psr.LOANAPPLICATIONID).ToList();
                var projectRiskRating = (from a in context.TBL_PROJECT_RISK_RATING
                                         join c in context.TBL_PROJECT_RISK_RATING_CATEGORY on a.CATEGORYID equals c.CATEGORYID
                                         where a.LOANAPPLICATIONID == psr.LOANAPPLICATIONID && a.LOANAPPLICATIONDETAILID == psr.LOANAPPLICATIONDETAILID
                                         select new
                                         {
                                             loanApplicationId = a.LOANAPPLICATIONID,
                                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                             loanBookingRequestId = a.LOANBOOKINGREQUESTID,
                                             categoryName = c.CATEGORYNAME,
                                             categoryValue = a.CATEGORYVALUE,
                                             projectLocation = a.PROJECTLOCATION,
                                             projectDetails = a.PROJECTDETAILS
                                         }).AsEnumerable().Select(a => new ProjectRiskRatingViewModel
                                         {
                                             loanApplicationId = a.loanApplicationId,
                                             loanApplicationDetailId = a.loanApplicationDetailId,
                                             loanBookingRequestId = a.loanBookingRequestId,
                                             categoryName = a.categoryName,
                                             categoryValue = a.categoryValue,
                                             projectLocation = a.projectLocation,
                                             projectDetails = a.projectDetails
                                         }).ToList();

                var result2 = projectRiskRating.Select(a => new ProjectRiskRatingViewModel
                {
                    loanApplicationId = a.loanApplicationId,
                    loanApplicationDetailId = a.loanApplicationDetailId,
                    loanBookingRequestId = a.loanBookingRequestId,
                    categoryName = a.categoryName,
                    categoryValue = a.categoryValue,
                    projectLocation = a.projectLocation,
                    projectDetails = a.projectDetails,
                    computation = context.TBL_PROJECT_RISK_RATING.Where(d => d.LOANAPPLICATIONDETAILID == a.loanApplicationDetailId).Sum(d => d.CATEGORYVALUE),
                }).ToList();

                foreach (var i in result2)
                {
                    int rating = 0;
                    var overRallTotal = i.computation + rating;
                    if (overRallTotal >= 81 && overRallTotal <= 100)
                    {
                        rec.projectRiskRating = "LOW";
                    }
                    if (overRallTotal >= 66 && overRallTotal < 81)
                    {
                        rec.projectRiskRating = "MODERATE";
                    }
                    if (overRallTotal >= 51 && overRallTotal < 66)
                    {
                        rec.projectRiskRating = "ABOVE AVERAGE";
                    }
                    if (overRallTotal < 51)
                    {
                        rec.projectRiskRating = "HIGH";
                    }
                }
            }
            return data;

        }

        public IEnumerable<PsrNextInspectionTaskViewModel> GetPsrNextInspectionTasks(int id)
        {
            return context.TBL_PSR_NEXT_INSPECTION_TASK.Where(x => x.DELETED == false && x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrNextInspectionTaskViewModel
                {
                    psrNextInspectionTaskId = x.PSRNEXTINSPECTIONTASKID,
                    comment = x.COMMENTS,
                    isDone = x.ISDONE,
                    inspectionDate = context.TBL_PSR_PROJECT_SITE_REPORT.Where(i => i.PROJECTSITEREPORTID == x.PROJECTSITEREPORTID).Select(i => i.INSPECTIONDATE).FirstOrDefault(),
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

        public IEnumerable<PsrImagesViewModel> GetPsrImages(int id)
        {
            return context.TBL_PSR_IMAGES.Where(x => x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrImagesViewModel
                {
                    psrImageId = x.PSRIMAGEID,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    imageCaption = x.IMAGECAPTION,
                    fileData = x.FILEDATA,
                }).ToList();
        }

        public IEnumerable<PsrCommentImagesViewModel> GetPsrCommentsImages(int id)
        {
            return context.TBL_PSR_COMMENT_IMAGES.Where(x => x.PROJECTSITEREPORTID == id)
                .Select(x => new PsrCommentImagesViewModel
                {
                    psrCommentImageId = x.PSRCOMMENTIMAGEID,
                    projectSiteReportId = x.PROJECTSITEREPORTID,
                    imageCaption = x.IMAGECAPTION,
                    fileData = x.FILEDATA,
                }).ToList();
        }

        public IEnumerable<ProjectSiteReportViewModel> GetPsrSignatories(int id)
        {

            var approval = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == id && x.OPERATIONID == (int)OperationsEnum.ProjectSiteReportApproval && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && x.APPROVALSTATEID == (int)ApprovalState.Ended).FirstOrDefault();
            var projectSite = context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            var staffName = context.TBL_STAFF.Find(projectSite.CREATEDBY);

            var officerName = " ";
            var groupHeadName = " ";
            officerName = staffName.FIRSTNAME + " " + staffName.MIDDLENAME + " " + staffName.LASTNAME;

            if (approval != null)
            {
                var groupHead = context.TBL_STAFF.Find(approval.REQUESTSTAFFID);
                if (groupHead == null)
                {
                    groupHeadName = "";
                }
                else
                {
                    groupHeadName = groupHead.FIRSTNAME + " " + groupHead.MIDDLENAME + " " + groupHead.LASTNAME;
                }
            }
            return context.TBL_PSR_PROJECT_SITE_REPORT.Where(x => x.PROJECTSITEREPORTID == id).Select(x => new ProjectSiteReportViewModel
            {
                projectOffer = officerName,
                groupHead = groupHeadName,
            }).ToList();
        }

        public IEnumerable<ProjectSiteReportViewModel> GetPsrSupervisorComment(int id)
        {
            var projectSite = context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            var staffName = context.TBL_STAFF.Find(projectSite.CREATEDBY);

            return context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == id && x.OPERATIONID == (int)OperationsEnum.ProjectSiteReportApproval).Select(x => new ProjectSiteReportViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                superComment = x.COMMENT
            }).OrderByDescending(x=>x.approvalTrailId).ToList().Take(1);
        }

        public IEnumerable<LoanApplicationViewModel> GetFacilities(int id)
        {
            var project = context.TBL_PSR_PROJECT_SITE_REPORT.Find(id);
            var currency = context.TBL_CURRENCY.Find(project.CURRENCYID);

            var dataTermLoan =  (from p in context.TBL_PSR_PROJECT_FACILITIES
                                join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                join r in context.TBL_LOAN on p.LOANID equals r.TERMLOANID
                                let loan_Application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                where p.PROJECTSITEREPORTID == id && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                                 select new LoanApplicationViewModel
                                {
                                customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                currency = currency.CURRENCYCODE,
                                applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                loanApplicationId = x.LOANAPPLICATIONID,
                                loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                moratrium = a.MORATORIUMDURATION,
                                equityControl = a.EQUITYAMOUNT,
                                pledgeCollateral = x.COLLATERALDETAIL,
                                loanPurpose = a.LOANPURPOSE,
                                valueOfCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(t => t.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.COLLATERALVALUE).FirstOrDefault(),
                                applicationDate = x.APPLICATIONDATE,
                                principalAmount = r.PRINCIPALAMOUNT == null ? 0 : r.PRINCIPALAMOUNT,
                                applicationAmount = x.APPLICATIONAMOUNT,
                                approvedAmount = a.APPROVEDAMOUNT,
                                interestRate = x.INTERESTRATE,
                                productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                tenor = a.APPROVEDTENOR,
                                relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                            }).ToList();

            var dataRevolving = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                join r in context.TBL_LOAN_REVOLVING on p.LOANID equals r.REVOLVINGLOANID
                                let loan_Application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                where p.PROJECTSITEREPORTID == id && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility
                                 select new LoanApplicationViewModel
                                {
                                    customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                    currency = currency.CURRENCYCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    moratrium = a.MORATORIUMDURATION,
                                    equityControl = a.EQUITYAMOUNT,
                                    pledgeCollateral = x.COLLATERALDETAIL,
                                    loanPurpose = a.LOANPURPOSE,
                                    principalAmount = r.OVERDRAFTLIMIT == null ? 0 : r.OVERDRAFTLIMIT,
                                    valueOfCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(t => t.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.COLLATERALVALUE).FirstOrDefault(),
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    interestRate = x.INTERESTRATE,
                                    productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    tenor = a.APPROVEDTENOR,
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                    isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                }).ToList();

            var dataContingent = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                join r in context.TBL_LOAN_CONTINGENT on p.LOANID equals r.CONTINGENTLOANID
                                let loan_Application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                where p.PROJECTSITEREPORTID == id && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability
                                  select new LoanApplicationViewModel
                                {
                                    customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                    currency = currency.CURRENCYCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    moratrium = a.MORATORIUMDURATION,
                                    equityControl = a.EQUITYAMOUNT,
                                    pledgeCollateral = x.COLLATERALDETAIL,
                                    principalAmount = r.CONTINGENTAMOUNT == null ? 0 : r.CONTINGENTAMOUNT,
                                    loanPurpose = a.LOANPURPOSE,
                                    valueOfCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(t => t.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.COLLATERALVALUE).FirstOrDefault(),
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    interestRate = x.INTERESTRATE,
                                    productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    tenor = a.APPROVEDTENOR,
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                    isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                }).ToList();

            var dataTermLoan2 = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                let loan_Application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                where p.PROJECTSITEREPORTID == id && p.LOANID == null && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                                 select new LoanApplicationViewModel
                                {
                                    customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                    currency = currency.CURRENCYCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    moratrium = a.MORATORIUMDURATION,
                                    equityControl = a.EQUITYAMOUNT,
                                    pledgeCollateral = x.COLLATERALDETAIL,
                                    loanPurpose = a.LOANPURPOSE,
                                    valueOfCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(t => t.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.COLLATERALVALUE).FirstOrDefault(),
                                    applicationDate = x.APPLICATIONDATE,
                                    principalAmount = 0,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    interestRate = x.INTERESTRATE,
                                    productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    tenor = a.APPROVEDTENOR,
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                    isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                }).ToList();

            var dataRevolving2 = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                 join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                 join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                 let loan_Application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                 where p.PROJECTSITEREPORTID == id && p.LOANID == null  && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility
                                 select new LoanApplicationViewModel
                                 {
                                     customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                     currency = currency.CURRENCYCODE,
                                     applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                     loanApplicationId = x.LOANAPPLICATIONID,
                                     loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                     moratrium = a.MORATORIUMDURATION,
                                     equityControl = a.EQUITYAMOUNT,
                                     pledgeCollateral = x.COLLATERALDETAIL,
                                     loanPurpose = a.LOANPURPOSE,
                                     principalAmount = 0,
                                     valueOfCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(t => t.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.COLLATERALVALUE).FirstOrDefault(),
                                     applicationDate = x.APPLICATIONDATE,
                                     applicationAmount = x.APPLICATIONAMOUNT,
                                     approvedAmount = a.APPROVEDAMOUNT,
                                     interestRate = x.INTERESTRATE,
                                     productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                     productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                     tenor = a.APPROVEDTENOR,
                                     relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                     relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                     relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                     relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                     operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                     isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                 }).ToList();

            var dataContingent2 = (from p in context.TBL_PSR_PROJECT_FACILITIES
                                  join x in context.TBL_LOAN_APPLICATION on p.LOANAPPLICATIONID equals x.LOANAPPLICATIONID
                                  join a in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                                  let loan_Application_detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == p.LOANAPPLICATIONID).Select(o => o).FirstOrDefault()
                                  where p.PROJECTSITEREPORTID == id && p.LOANID == null  && p.LANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability
                                  select new LoanApplicationViewModel
                                  {
                                      customerName = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == a.CUSTOMERID).Select(o => o.LASTNAME + " " + o.FIRSTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                      currency = currency.CURRENCYCODE,
                                      applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                      loanApplicationId = x.LOANAPPLICATIONID,
                                      loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                      moratrium = a.MORATORIUMDURATION,
                                      equityControl = a.EQUITYAMOUNT,
                                      pledgeCollateral = x.COLLATERALDETAIL,
                                      principalAmount = 0,
                                      loanPurpose = a.LOANPURPOSE,
                                      valueOfCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(t => t.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.COLLATERALVALUE).FirstOrDefault(),
                                      applicationDate = x.APPLICATIONDATE,
                                      applicationAmount = x.APPLICATIONAMOUNT,
                                      approvedAmount = a.APPROVEDAMOUNT,
                                      interestRate = x.INTERESTRATE,
                                      productTypeId = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTTYPEID).FirstOrDefault(),
                                      productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                      tenor = a.APPROVEDTENOR,
                                      relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                      relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                      relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                      relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                      operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                      isProjectRelated = x.ISPROJECTRELATED == true ? "YES" : "NO"
                                  }).ToList();

            var data1 = dataTermLoan.Union(dataRevolving).Union(dataContingent);
            var data2 = dataTermLoan2.Union(dataRevolving2).Union(dataContingent2);
            var data = data1.Union(data2);

            foreach (var item in data)
            {
                if (item.productTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
                {
                    var utilizations = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.OVERDRAFTLIMIT);
                    }
                    else item.totalUtilized = (decimal)0;
                }
                if (item.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                {
                    var utilizations = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.CONTINGENTAMOUNT);
                    }
                    else item.totalUtilized = (decimal)0;

                }
                if (item.productTypeId != (short)LoanProductTypeEnum.RevolvingLoan && item.productTypeId != (short)LoanProductTypeEnum.ContingentLiability)
                {
                    var utilizations = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.LOANSTATUSID == (int)LoanStatusEnum.Active).ToList();
                    if (utilizations.Count() > 0)
                    {
                        item.totalUtilized = utilizations.Sum(x => x.PRINCIPALAMOUNT);
                    }
                    else item.totalUtilized = (decimal)0;
                }
            }
            return data;
        }

        public IEnumerable<InterestIncomeViewModel> GetInterestIncome(DateTime startDate, DateTime endDate)
        {
            var dataTermLoan = (from p in context.TBL_LOAN
                                join x in context.TBL_DAILY_ACCRUAL on p.LOANREFERENCENUMBER equals x.REFERENCENUMBER
                                join e in context.TBL_LOAN_PRUDENTIALGUIDELINE on p.USER_PRUDENTIAL_GUIDE_STATUSID equals e.PRUDENTIALGUIDELINESTATUSID
                                where (DbFunctions.TruncateTime(p.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(p.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                select new
                                {
                                    referenceNumber = p.LOANREFERENCENUMBER,
                                    dateTimeCreated = DbFunctions.TruncateTime(p.DATETIMECREATED).Value,
                                    dailyAccrualAmount = x.DAILYACCURALAMOUNT,
                                    prudentialGuideLineTypeId = e.PRUDENTIALGUIDELINETYPEID,
                                }).AsEnumerable().Select(O => new
                                {
                                    period = O.dateTimeCreated.ToString("MMMM, yyyy"),
                                    dailyAccrualAmount = O.dailyAccrualAmount,
                                    prudentialGuideLineTypeId = O.prudentialGuideLineTypeId,
                                }).ToList();

            var dataRevolving = (from p in context.TBL_LOAN_REVOLVING
                                 join x in context.TBL_DAILY_ACCRUAL on p.LOANREFERENCENUMBER equals x.REFERENCENUMBER
                                 join e in context.TBL_LOAN_PRUDENTIALGUIDELINE on p.USER_PRUDENTIAL_GUIDE_STATUSID equals e.PRUDENTIALGUIDELINESTATUSID
                                 where (DbFunctions.TruncateTime(p.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(p.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                 select new
                                 {
                                     referenceNumber = p.LOANREFERENCENUMBER,
                                     dateTimeCreated = DbFunctions.TruncateTime(p.DATETIMECREATED).Value,
                                     dailyAccrualAmount = x.DAILYACCURALAMOUNT,
                                     prudentialGuideLineTypeId = e.PRUDENTIALGUIDELINETYPEID,
                                 }).AsEnumerable().Select(O => new
                                 {
                                     period = O.dateTimeCreated.ToString("MMMM, yyyy"),
                                     dailyAccrualAmount = O.dailyAccrualAmount,
                                     prudentialGuideLineTypeId = O.prudentialGuideLineTypeId,
                                 }).ToList();

            var result = dataTermLoan.Union(dataRevolving).GroupBy(O => O.period).Select(O => new InterestIncomeViewModel
            {
                period = O.FirstOrDefault().period,
                performing = O.Where(t => t.prudentialGuideLineTypeId == (int)PrudentialGuidelineTypeEnum.Performing).Sum(t => t.dailyAccrualAmount),
                nonPerforming = O.Where(t => t.prudentialGuideLineTypeId == (int)PrudentialGuidelineTypeEnum.NonPerforming).Sum(t => t.dailyAccrualAmount),
                totalMonthlyIncome = O.Sum(t => t.dailyAccrualAmount)
            }).ToList();

            foreach (var item in result)
            {
                item.totalIncome = result.Sum(O => O.totalMonthlyIncome);
            }

            return result;
        }

        public IEnumerable<FixedDepositCollateralViewModel> GetFixedDepositCollaterals(int companyId, string customerCode)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from d in context.TBL_COLLATERAL_CUSTOMER
                           join e in context.TBL_COLLATERAL_DEPOSIT on d.COLLATERALCUSTOMERID equals e.COLLATERALCUSTOMERID
                           join f in context.TBL_CUSTOMER on d.CUSTOMERID equals f.CUSTOMERID
                           join g in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals g.LOANAPPLICATIONID
                           join h in context.TBL_LOAN_APPLICATION_DETAIL on g.LOANAPPLICATIONID equals h.LOANAPPLICATIONID
                           where f.CUSTOMERCODE == customerCode && d.COLLATERALTYPEID == (int) CollateralTypeEnum.FixedDeposit
                           orderby d.DATETIMECREATED descending
                           select new FixedDepositCollateralViewModel()
                           {
                               collateralSummary = d.COLLATERALSUMMARY,
                               valuationCycle = d.VALUATIONCYCLE.ToString(),
                               dateTimeCreated = d.DATETIMECREATED,
                               collateralType = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                               //collateralDetail = d.TBL_COLLATERAL_TYPE.DETAILS,
                               collateralCode = d.COLLATERALCODE,
                               collateralValue = d.COLLATERALVALUE,
                               effectiveDate = e.EFFECTIVEDATE,
                               maturityDate = e.MATURITYDATE,
                               accountNumber = e.ACCOUNTNUMBER,
                               securityValue = e.SECURITYVALUE,
                               availableBalance = e.AVAILABLEBALANCE,
                               customerCode = f.CUSTOMERCODE,
                               customerName = f.FIRSTNAME + " " + f.LASTNAME,

                               applicationReference = g.APPLICATIONREFERENCENUMBER,
                               comment = e.REMARK,
                               contractCode = e.DEALREFERENCENUMBER,
                               currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                               facilityDetails = h.LOANPURPOSE,
                               facilityReference = null,
                               facilityType = h.TBL_PRODUCT.PRODUCTNAME,
                           };

                var data2 = from d in context.TBL_COLLATERAL_CUSTOMER
                           join e in context.TBL_COLLATERAL_DEPOSIT on d.COLLATERALCUSTOMERID equals e.COLLATERALCUSTOMERID
                           join f in context.TBL_CUSTOMER on d.CUSTOMERID equals f.CUSTOMERID
                           join g in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals g.LOANAPPLICATIONID
                           join h in context.TBL_LOAN_APPLICATION_DETAIL on g.LOANAPPLICATIONID equals h.LOANAPPLICATIONID
                           join i in context.TBL_LOAN on h.LOANAPPLICATIONDETAILID equals i.LOANAPPLICATIONDETAILID
                           where f.CUSTOMERCODE == customerCode && d.COLLATERALTYPEID == (int)CollateralTypeEnum.FixedDeposit && i.ISDISBURSED == true
                           orderby d.DATETIMECREATED descending
                           select new FixedDepositCollateralViewModel()
                           {
                               collateralSummary = d.COLLATERALSUMMARY,
                               valuationCycle = d.VALUATIONCYCLE.ToString(),
                               dateTimeCreated = d.DATETIMECREATED,
                               collateralType = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                               //collateralDetail = d.TBL_COLLATERAL_TYPE.DETAILS,
                               collateralCode = d.COLLATERALCODE,
                               collateralValue = d.COLLATERALVALUE,
                               effectiveDate = e.EFFECTIVEDATE,
                               maturityDate = e.MATURITYDATE,
                               accountNumber = e.ACCOUNTNUMBER,
                               securityValue = e.SECURITYVALUE,
                               availableBalance = e.AVAILABLEBALANCE,
                               customerCode = f.CUSTOMERCODE,
                               customerName = f.FIRSTNAME + " " + f.LASTNAME,

                               applicationReference = g.APPLICATIONREFERENCENUMBER,
                               comment = e.REMARK,
                               contractCode = e.DEALREFERENCENUMBER,
                               currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                               facilityDetails = h.LOANPURPOSE,
                               facilityReference = i.LOANREFERENCENUMBER,
                               facilityType = h.TBL_PRODUCT.PRODUCTNAME,
                           };

                data = data.Where(O => !data2.Any(N => N.collateralCode.Equals(O.collateralCode)));
                var result = data.Union(data2).ToList(); //.GroupBy(O => O.collateralCode).FirstOrDefault()
                return result;
            }
        }

        public IEnumerable<FixedDepositCollateralViewModel> GetValidCollaterals(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from d in context.TBL_COLLATERAL_CUSTOMER
                           join f in context.TBL_CUSTOMER on d.CUSTOMERID equals f.CUSTOMERID
                           where (DbFunctions.TruncateTime(d.VALIDTILL) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(d.VALIDTILL) <= DbFunctions.TruncateTime(endDate))
                           orderby d.DATETIMECREATED descending
                           select new FixedDepositCollateralViewModel()
                           {
                               collateralSummary = d.COLLATERALSUMMARY,
                               valuationCycle = d.VALUATIONCYCLE.ToString(),
                               dateTimeCreated = d.DATETIMECREATED,
                               collateralType = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                               //collateralDetail = d.TBL_COLLATERAL_TYPE.DETAILS,
                               collateralCode = d.COLLATERALCODE,
                               collateralValue = d.COLLATERALVALUE,
                               validityExpiryDate = d.VALIDTILL.Value,
                               customerCode = f.CUSTOMERCODE,
                               customerName = f.FIRSTNAME + " " + f.LASTNAME,
                               accountNumber = context.TBL_CASA.Where(O => O.CUSTOMERID == f.CUSTOMERID).FirstOrDefault().PRODUCTACCOUNTNUMBER,
                           };

                return data.ToList();
            }
        }

    }
}
