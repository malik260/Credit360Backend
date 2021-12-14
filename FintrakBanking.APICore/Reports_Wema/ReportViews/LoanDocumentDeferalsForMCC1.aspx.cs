using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FintrakBanking.Common.Extensions;
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
                try
                {

                    int? branchCode = 0;
                    DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                    int companyId = Int32.Parse(Request.QueryString["companyId"]);
                    string branch = Request.QueryString["branchId"];
                    if (branch != null)
                        branchCode = Int32.Parse(Request.QueryString["branchId"]);
                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];

                    HashHelper hash = new HashHelper();

                    DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    if (inputHashValue != incomingDateHash)
                    {
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                        return;
                    }

                    var currentDate = DateTime.Now;

                    var dateDifference = currentDate - incomingDate;

                    if (dateDifference.Seconds > 10)
                    {
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                        return;
                    }



                    LoanReportObjects dispursement = new LoanReportObjects();
                    var loanDocumentWaivedForMCC = dispursement.LoanDocumentWaivedForMCC(startDate, companyId, branchCode);
                    var loanDeferralMCCExp = dispursement.LoanDeferralMCCExp(startDate, companyId, branchCode);
                    var loanDeferralMCCCur = dispursement.LoanDeferralMCCCur(startDate, companyId, branchCode);

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