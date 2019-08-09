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
                        fieldInfo.SetValue(extension, false);
                    }

                    RSR psr = new RSR();

                    var projectSiteRepor = psr.GetProjectSiteReports(projectSiteReportId);
                    ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = projectSiteRepor;
                    reportDataSource.Name = "detail";


                    //var facilities = psr.GetFacilities(projectSiteReportId);
                    //ReportDataSource dsOfferLetterDetails = new ReportDataSource();
                    //dsOfferLetterDetails.Value = facilities;
                    //dsOfferLetterDetails.Name = "OfferLetterDetails";

                    var performaceEvaluation = psr.GetPsrPerformanceEvaluations(projectSiteReportId);
                    ReportDataSource dsPerformaceEvaluation = new ReportDataSource();
                    dsPerformaceEvaluation.Value = performaceEvaluation;
                    dsPerformaceEvaluation.Name = "detail";


                    var observations = psr.GetPsrObservations(projectSiteReportId);
                    ReportDataSource dsObservations = new ReportDataSource();
                    dsObservations.Value = observations;
                    dsObservations.Name = "observation";

                    var comment = psr.GetPsrComments(projectSiteReportId);
                    ReportDataSource dsComment = new ReportDataSource();
                    dsComment.Value = comment;
                    dsComment.Name = "comment";

                    var nextInspection = psr.GetPsrNextInspectionTasks(projectSiteReportId);
                    ReportDataSource dsNextInspection = new ReportDataSource();
                    dsNextInspection.Value = nextInspection;
                    dsNextInspection.Name = "nextInspection";


                    this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
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