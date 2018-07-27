using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FintrakBanking.ReportObjects;
using Microsoft.Reporting.WebForms;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class LoanDocumentDeferalsForMCC : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                int companyId = Int32.Parse(Request.QueryString["companyId"]);
                string branch = Request.QueryString["branchId"];
              int  branchCode = Int32.Parse(Request.QueryString["branchId"]);


                LoanReportObjects dispursement = new LoanReportObjects();
                var loanDocumentWaivedForMCC = dispursement.LoanDocumentWaivedForMCC(startDate,companyId, branchCode);
                var loanDeferralMCCExp = dispursement.LoanDeferralMCCExp(startDate,companyId, branchCode);
                var loanDeferralMCCCur = dispursement.LoanDeferralMCCCur(startDate,companyId, branchCode);

                this.ReportViewer.LocalReport.DataSources.Clear();
                ReportDataSource reportDataSource1 = new ReportDataSource();
                ReportDataSource reportDataSource2 = new ReportDataSource();
                ReportDataSource reportDataSource3 = new ReportDataSource();
                reportDataSource1.Value = loanDocumentWaivedForMCC;
                reportDataSource2.Value = loanDeferralMCCExp;
                reportDataSource3.Value = loanDeferralMCCCur;
                reportDataSource1.Name = "WaiverMCC";
                reportDataSource2.Name = "DeferralMCCExp";
                reportDataSource2.Name = "DeferralMCC";

                ReportParameter sDate = new ReportParameter("startDate", startDate.ToString());

                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource1);
                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource2);
                this.ReportViewer.LocalReport.DataSources.Add(reportDataSource3);
                this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/LoanDocumentDeferralForMCC.rdlc");
                ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate });
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}