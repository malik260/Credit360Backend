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
    public partial class PostedTransactions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
            DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
            int companyId = Int32.Parse(Request.QueryString["companyId"]);
            int PostedByStaffId = Int32.Parse(Request.QueryString["PostedByStaffId"]);
            int glAccountId = Int32.Parse(Request.QueryString["glAccountId"]);
            int branchId = Int32.Parse(Request.QueryString["branchId"]);

            FinanceRepotObject tran = new FinanceRepotObject();
            var data = tran.FinanceTransaction(startDate, endDate, companyId,branchId,glAccountId,PostedByStaffId);

            this.ReportViewer.LocalReport.DataSources.Clear();
            ReportDataSource reportDataSource = new ReportDataSource();
            reportDataSource.Value = data;
            reportDataSource.Name = "FacilityApp";

            this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
            this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/FinanceTransactions.rdlc");
            this.ReportViewer.LocalReport.Refresh();

            ReportViewer.LocalReport.Refresh();
        }
    }
}