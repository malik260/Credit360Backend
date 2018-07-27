using FintrakBanking.ReportObjects.ReportingObjects;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class CollateralInsurance : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);

                LimitsMonitoringReportsObjects sla = new LimitsMonitoringReportsObjects();
                var data = sla.InsuranceApprochingExpiration(startDate, endDate);

                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Value = data;
                reportDataSource.Name = "Insurance";

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/CollateralInsuranceReport.rdlc");
                this.ReportViewer.LocalReport.Refresh();

            }
        }
    }
}