using FintrakBanking.Common.Extensions;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class OverLineReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                try
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    string postedDate = Request.QueryString["runDate"];
                    DateTime RunDate = Convert.ToDateTime(postedDate);
                    string rDate = RunDate.Year + "-" + RunDate.Month + "-" + RunDate.Day;
                    string Level = Request.QueryString["level"];
                    string MisCode = Request.QueryString["misCode"];
                    string ExposureType = Request.QueryString["exposureType"];
                    string DivisionName = Request.QueryString["divisionName"];
                    string GroupName = Request.QueryString["groupName"];
                    string BranchName = Request.QueryString["branchName"];
                    string RegionName = Request.QueryString["regionName"];

                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];

                    HashHelper hash = new HashHelper();

                    DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    if (inputHashValue != incomingDateHash)
                    {
                        this.ReportViewer05.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer05.LocalReport.Refresh();
                        return;
                    }

                    var currentDate = DateTime.Now;

                    var dateDifference = currentDate - incomingDate;

                    if (dateDifference.Seconds > 30)
                    {
                        this.ReportViewer05.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer05.LocalReport.Refresh();
                        return;
                    }




                    string reportServerUrl = ConfigurationManager.AppSettings["ReportServerURL"];
                    string domain = ConfigurationManager.AppSettings["rsDomain"];
                    string userName = ConfigurationManager.AppSettings["rsUserName"];
                    string password = ConfigurationManager.AppSettings["rsPassword"];
                    string reportPath = ConfigurationManager.AppSettings["ServerReportPath"];


                    ReportViewer05.ServerReport.ReportServerUrl = new Uri(reportServerUrl);
                    ReportViewer05.ServerReport.ReportServerCredentials = new ReportServerCredentials(userName, password, domain);
                    ReportViewer05.ServerReport.ReportPath = reportPath + "Overline Report"; // string.Format(reportPath, "Risk Asset");

                    ReportViewer05.ProcessingMode = ProcessingMode.Remote;
                    ReportViewer05.ShowCredentialPrompts = false;

                    ReportParameter[] reportParameter = new ReportParameter[8];
                    reportParameter[0] = new ReportParameter("MisCode", MisCode);
                    reportParameter[1] = new ReportParameter("Level", Level);
                    reportParameter[2] = new ReportParameter("ExposureType", ExposureType);
                    reportParameter[3] = new ReportParameter("DivisionName", DivisionName);
                    reportParameter[4] = new ReportParameter("GroupName", GroupName);
                    reportParameter[5] = new ReportParameter("BranchName", BranchName);
                    reportParameter[6] = new ReportParameter("RunDate", rDate);
                    reportParameter[7] = new ReportParameter("RegionName", RegionName);

                    //==== NOTE: for report on server, use the below ============
                    ReportViewer05.ServerReport.SetParameters(reportParameter);
                    ReportViewer05.ServerReport.Refresh();


                }
                catch (Exception ex)
                {
                    this.ReportViewer05.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.ReportViewer05.LocalReport.Refresh();
                    return;
                }
            }
        }
    }
}