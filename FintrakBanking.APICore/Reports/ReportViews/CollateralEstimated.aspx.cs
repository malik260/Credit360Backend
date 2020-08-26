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
    public partial class CollateralEstimated : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
      {

            if (!IsPostBack)
            {
                try
                {
                    int companyId = Int32.Parse(Request.QueryString["companyId"]);
                    string collateralCode = Request.QueryString["collateralCode"];
                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];

                    HashHelper hash = new HashHelper();
                    DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);
                    var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    //if (inputHashValue != incomingDateHash)
                    //{
                    //    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    //    this.ReportViewer.LocalReport.Refresh();
                    //    return;
                    //}

                    var currentDate = DateTime.Now;
                    var dateDifference = currentDate - incomingDate;

                    if (dateDifference.Seconds > 10)
                    {
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                        return;
                    }

                    //string logo = Server.MapPath("~/Content/icons/firstbank.png");
                    //LoanReportObjects loanRepo = new LoanReportObjects();
                    var data = LoanReportObjects.CollateralEstimated(companyId, collateralCode);

                    this.ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = data;
                    reportDataSource.Name = "CollateralEstimated";

                    this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/CollateralEstimated.rdlc");
                    this.ReportViewer.LocalReport.Refresh();
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