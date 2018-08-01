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
    public partial class DailyTransactionReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
               
                DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
                string searchParamemter = Request.QueryString["searchParamemter"];
                int companyId = Int32.Parse(Request.QueryString["companyId"]);
               


                FinanceRepotObject accru = new FinanceRepotObject();
                var data = accru.DailyAccrual(endDate, startDate, companyId, searchParamemter);

                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Value = data;
                reportDataSource.Name = "accrual";

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/DailyAccrualReport.rdlc");

                ReportParameter sDate = new ReportParameter("startDate", startDate.ToString());
                ReportParameter eDate = new ReportParameter("endDate", endDate.ToString());
             //   ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate });
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}