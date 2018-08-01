using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Entities.Models;
using FintrakBanking.ReportObjects.ReportingObjects;
using FintrakBanking.Repositories.Setups.General;
using Microsoft.Reporting.WebForms;
using System;

namespace FintrakBanking.APICore.Reports.Credit.Monitoring
{
    public partial class CollateralPropertyRevaluation : System.Web.UI.Page
    {
        private ApiControllerBase authCtrl = new ApiControllerBase();

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);

                LimitsMonitoringReportsObjects report = new LimitsMonitoringReportsObjects();
                var data = report.CollateralPropertyRevaluation(startDate, endDate);

                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Value = data;
                reportDataSource.Name = "CollateralPropertyDetails";

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/CollateralPropertyRevaluation.rdlc");
                this.ReportViewer.LocalReport.Refresh();
            }

        }

    }

}