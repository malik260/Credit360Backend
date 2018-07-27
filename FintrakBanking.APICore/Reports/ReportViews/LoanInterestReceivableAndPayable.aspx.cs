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
    public partial class LoanReceivableAndPayable : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
                int companyId = Int32.Parse(Request.QueryString["companyId"]);
                int productClassId = Int32.Parse(Request.QueryString["productClassId"]);
                string searchParamemter = Request.QueryString["searchParamemter"];

                LoanReportObjects sla = new LoanReportObjects();
                var data = sla.GetLoansInterestReceivable(startDate, endDate,companyId, searchParamemter,productClassId);

                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Value = data;
                reportDataSource.Name = "CommercialLoan";

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/LoanInterestReceivableAndPayable.rdlc");

                ReportParameter sDate = new ReportParameter("startDate", startDate.ToString());
                ReportParameter eDate = new ReportParameter("endDate", endDate.ToString());

                ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate });
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}