
using FintrakBanking.ReportObjects.ReportingObjects;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class PostedFinancialTransactions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string imagePath = new Uri(Server.MapPath("~/Content/icons/firstbank-logo.jpg")).AbsoluteUri;

              DateTime  StartDate = DateTime.Parse( Request.QueryString["StartDate"]);
                DateTime EndDate = DateTime.Parse(Request.QueryString["EndDate"]);
                int PostedByStaffId = int.Parse(Request.QueryString["PostedByStaffId"]);
                int glAccountId = int.Parse(Request.QueryString["glAccountId"]);
                int branchId = int.Parse(Request.QueryString["branchId"]);
                int companyId = int.Parse(Request.QueryString["companyId"]);

                FinanceRepotObject sla = new FinanceRepotObject();
                var data = sla.FinanceTransaction(StartDate, EndDate, companyId,branchId,glAccountId,PostedByStaffId);

                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Value = data;
                reportDataSource.Name = "FinanceTransactions";

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/FinanceTransactions.rdlc");


                ReportParameter sDate = new ReportParameter("StartDate", StartDate.ToString());
                ReportParameter eDate = new ReportParameter("EndDate", EndDate.ToString());
                ReportParameter logoPath = new ReportParameter("logo", imagePath);

                this.ReportViewer.LocalReport.EnableExternalImages = true;
                ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate, logoPath });
                ReportViewer.LocalReport.Refresh();

            }
        }
    }
}