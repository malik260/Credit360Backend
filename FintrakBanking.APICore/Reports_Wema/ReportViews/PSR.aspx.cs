using FintrakBanking.Common.Extensions;
using FintrakBanking.ReportObjects.ReportingObjects;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class RACaspx : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    int projectSiteReportId = Int32.Parse(Request.QueryString["projectSiteReportId"]);
                    int psrReportTypeId = Int32.Parse(Request.QueryString["psrReportTypeId"]);

                    HashHelper hash = new HashHelper();

                    string exportOption = "PDF";
                    RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                    if (extension != null)
                    {
                        System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        fieldInfo.SetValue(extension, true);
                    }

                    RSR psr = new RSR();

                    var projectSiteRepor = psr.GetProjectSiteReports(projectSiteReportId);
                    ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = projectSiteRepor;
                    reportDataSource.Name = "detail";
                    ReportViewer.LocalReport.DataSources.Add(reportDataSource);

                    var performaceEvaluation = psr.GetPsrPerformanceEvaluations(projectSiteReportId);
                    ReportDataSource dsPerformaceEvaluation = new ReportDataSource();
                    dsPerformaceEvaluation.Value = performaceEvaluation;
                    dsPerformaceEvaluation.Name = "evaluation";
                    ReportViewer.LocalReport.DataSources.Add(dsPerformaceEvaluation); 

                    var apg = psr.GetProjectSiteReportsApg(projectSiteReportId);
                    ReportDataSource dsApg = new ReportDataSource();
                    dsApg.Value = apg;
                    dsApg.Name = "apg";
                    ReportViewer.LocalReport.DataSources.Add(dsApg);

                    var analysis = psr.GetPsrPerformanceAnalysis(projectSiteReportId);
                    ReportDataSource dsAnalysis = new ReportDataSource();
                    dsAnalysis.Value = analysis;
                    dsAnalysis.Name = "analysis";
                    ReportViewer.LocalReport.DataSources.Add(dsAnalysis);

                   
                    var facilities = psr.GetFacilities(projectSiteReportId);
                    ReportDataSource dsfacilities = new ReportDataSource();
                    dsfacilities.Value = facilities;
                    dsfacilities.Name = "facilities";
                    ReportViewer.LocalReport.DataSources.Add(dsfacilities);

                    var observations = psr.GetPsrObservations(projectSiteReportId);
                    ReportDataSource dsObservations = new ReportDataSource();
                    dsObservations.Value = observations;
                    dsObservations.Name = "observation"; 
                    ReportViewer.LocalReport.DataSources.Add(dsObservations);

                    var recomendations = psr.Getrecomendations(projectSiteReportId);
                    ReportDataSource dsrecomendations = new ReportDataSource();
                    dsrecomendations.Value = recomendations;
                    dsrecomendations.Name = "recomendations"; 
                    ReportViewer.LocalReport.DataSources.Add(dsrecomendations);

                    var comment = psr.GetPsrComments(projectSiteReportId);
                    ReportDataSource dsComment = new ReportDataSource();
                    dsComment.Value = comment;
                    dsComment.Name = "comment";
                    ReportViewer.LocalReport.DataSources.Add(dsComment);

                    var nextInspection = psr.GetPsrNextInspectionTasks(projectSiteReportId);
                    ReportDataSource dsNextInspection = new ReportDataSource();
                    dsNextInspection.Value = nextInspection;
                    dsNextInspection.Name = "nextInspection";
                    ReportViewer.LocalReport.DataSources.Add(dsNextInspection);

                    var image = psr.GetPsrImages(projectSiteReportId);
                    ReportDataSource dsImage = new ReportDataSource();
                    dsImage.Value = image;
                    dsImage.Name = "supportImages";
                    ReportViewer.LocalReport.DataSources.Add(dsImage);

                    var commentImages = psr.GetPsrCommentsImages(projectSiteReportId);
                    ReportDataSource commentImage = new ReportDataSource();
                    commentImage.Value = commentImages;
                    commentImage.Name = "commentimages";
                    ReportViewer.LocalReport.DataSources.Add(commentImage);

                    var signatories = psr.GetPsrSignatories(projectSiteReportId);
                    ReportDataSource signatoriy = new ReportDataSource();
                    signatoriy.Value = signatories;
                    signatoriy.Name = "signatories";
                    ReportViewer.LocalReport.DataSources.Add(signatoriy);

                    var supervisorComment = psr.GetPsrSupervisorComment(projectSiteReportId);
                    ReportDataSource superComment = new ReportDataSource();
                    superComment.Value = supervisorComment;
                    superComment.Name = "supervisorcomment";
                    ReportViewer.LocalReport.DataSources.Add(superComment);


                    if (psrReportTypeId == 1)
                    {
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/PSR_Direct.rdlc");
                    }
                    else { this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/PSR_APG.rdlc"); }
                    
                    this.ReportViewer.LocalReport.Refresh();
                }
                catch (Exception ex)
                {
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.ReportViewer.LocalReport.Refresh();
                    return;
                }

            }
        }
    }
}