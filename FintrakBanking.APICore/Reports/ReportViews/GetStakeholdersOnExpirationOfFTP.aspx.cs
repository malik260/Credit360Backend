using FintrakBanking.ReportObjects;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class GetStakeholdersOnExpirationOfFTP : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                short branchId = 0;
                DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                string branch = Request.QueryString["branchId"];
                if (branch != null && branch != "")
                    branchId = short.Parse(Request.QueryString["branchId"]);
                string customerName = Request.QueryString["customerName"];

                LoanReportObjects sla = new LoanReportObjects();
                var data = sla.GetStakeHolderOnExperationOfFTP(branchId, customerName, startDate);

                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Value = data;
                reportDataSource.Name = "Stakeholder";

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/StakeholderWithExpiredFTP.rdlc");
                this.ReportViewer.LocalReport.Refresh();
            }

        }
    }
}