using FintrakBanking.Common.Extensions;
using FintrakBanking.ReportObjects;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class StalledPerfection : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                try
                {
                    DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                    DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
                    int companyId = Int32.Parse(Request.QueryString["companyId"]);
                    //short branchId = short.Parse(Request.QueryString["branchId"]);
                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];

                    HashHelper hash = new HashHelper();

                    DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    if (inputHashValue != incomingDateHash)
                    {
                        this.ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer1.LocalReport.Refresh();
                        return;
                    }

                    var currentDate = DateTime.Now;

                    var dateDifference = currentDate - incomingDate;

                    if (dateDifference.Seconds > 30)
                    {
                        this.ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer1.LocalReport.Refresh();
                        return;
                    }
                    LoanReportObjects dispursement = new LoanReportObjects();
                    var data = dispursement.StalledPerfectionForCollateral(startDate, endDate, companyId);

                    this.ReportViewer1.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = data;
                    reportDataSource.Name = "StalledPerfectionDataSet";

                    ReportParameter sDate = new ReportParameter("startDate", startDate.ToString());
                    ReportParameter eDate = new ReportParameter("endDate", endDate.ToString());

                    this.ReportViewer1.LocalReport.DataSources.Add(reportDataSource);
                    this.ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/StalledPerfection.rdlc");
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate });
                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception ex)
                {
                    this.ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.ReportViewer1.LocalReport.Refresh();
                    return;
                }
            }



        }
    }
}