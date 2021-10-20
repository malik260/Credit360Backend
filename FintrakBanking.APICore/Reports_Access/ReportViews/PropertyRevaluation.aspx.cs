using FintrakBanking.ReportObjects.Credit;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class PropertyRevaluation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoanMonitoring logic = new LoanMonitoring();
                string companyId = Request.QueryString["companyId"];
                string value = Request.QueryString["value"];
                int companyIntId = 0;
                int valueInt = 0;
                if (!string.IsNullOrEmpty(companyId))
                {
                    companyIntId = Convert.ToInt32(companyId);
                }
                if (!string.IsNullOrEmpty(value))
                {
                    valueInt = Convert.ToInt32(value);
                }
                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Value = logic.CollateralPropertyRevaluation(companyIntId,valueInt);
                reportDataSource.Name = "revaluationReport";

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/CollateralRevaluation.rdlc");
                this.ReportViewer.LocalReport.Refresh();
            }
        }
    }
}