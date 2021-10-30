using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FintrakBanking.Common.Extensions;
using System.Globalization;
using Microsoft.Reporting.WebForms;


namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class RiskAssetsReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack) 
            {
                try
                {
                    //CultureInfo provider = CultureInfo.InvariantCulture;
                    //string postedDate = Request.QueryString["runDate"];
                    //DateTime RunDate = Convert.ToDateTime(postedDate);               
                    //string rDate = RunDate.Year+"-"+RunDate.Month+"-"+RunDate.Day;
                    //string Level = Request.QueryString["level"];
                    //string MisCode = Request.QueryString["misCode"];
                    //string ExposureType = Request.QueryString["exposureType"];
                    //string DivisionName = Request.QueryString["divisionName"];
                    //string GroupName = Request.QueryString["groupName"];
                    //string BranchName = Request.QueryString["branchName"];
                    //string RegionName = Request.QueryString["regionName"];

                    //string inputDateInfo = Request.QueryString["key1"];
                    //string inputHashValue = Request.QueryString["key2"];

                    //HashHelper hash = new HashHelper();

                    //DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    //var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    //if (inputHashValue != incomingDateHash)
                    //{
                    //    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    //    this.ReportViewer.LocalReport.Refresh();
                    //    return;
                    //}

                    //var currentDate = DateTime.Now;

                    //var dateDifference = currentDate - incomingDate;

                    //if (dateDifference.Seconds > 30)
                    //{
                    //    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    //    this.ReportViewer.LocalReport.Refresh();
                    //    return;
                    //}

                    string reportServerUrl = ConfigurationManager.AppSettings["ReportServerURL"];
                    string domain = ConfigurationManager.AppSettings["rsDomain"];
                    string userName = ConfigurationManager.AppSettings["rsUserName"]; 
                    string password = ConfigurationManager.AppSettings["rsPassword"];  
                    string reportPath = ConfigurationManager.AppSettings["ServerReportPath"];

                    this.ReportViewer.ServerReport.ReportServerUrl = new Uri(reportServerUrl);
                    this.ReportViewer.ServerReport.ReportServerCredentials = new ReportServerCredentials(userName, password, domain);
                    this.ReportViewer.ServerReport.ReportPath = reportPath + "RISK ASSET";
                    //this.ReportViewer.ServerReport.ReportPath = string.Format(reportPath, "RISK ASSET");

                    this.ReportViewer.ShowCredentialPrompts = false;
                    this.ReportViewer.ProcessingMode = ProcessingMode.Remote;

                    ReportParameter[] reportParameter = new ReportParameter[8];
                    reportParameter[0] = new ReportParameter("MisCode", "BAC800");
                    reportParameter[1] = new ReportParameter("Level", "4");
                    reportParameter[2] = new ReportParameter("ExposureType", "DIRECT");
                    reportParameter[3] = new ReportParameter("RunDate", "2019-08-31"); // yyyy-mm-dd
                    reportParameter[4] = new ReportParameter("DivisionName", "BUSINESS BANKING DIVISION");
                    reportParameter[5] = new ReportParameter("GroupName", "Business Banking Abuja & North");
                    reportParameter[6] = new ReportParameter("BranchName", "BBD Branch_Business Banking Team (Garki)");
                    reportParameter[7] = new ReportParameter("RegionName", "Business Banking Abuja Cadastral Zone");

                    //ReportParameter[] reportParameter = new ReportParameter[8];
                    //reportParameter[0] = new ReportParameter("MisCode", MisCode);
                    //reportParameter[1] = new ReportParameter("Level", Level);
                    //reportParameter[2] = new ReportParameter("ExposureType", ExposureType);
                    //reportParameter[3] = new ReportParameter("RunDate", rDate); // yyyy-mm-dd

                    //reportParameter[4] = new ReportParameter("DivisionName", DivisionName); 
                    //reportParameter[5] = new ReportParameter("GroupName", GroupName); 
                    //reportParameter[6] = new ReportParameter("BranchName", BranchName);                     
                    //reportParameter[7] = new ReportParameter("RegionName", RegionName); 

                    //==== NOTE: for report on server, use the below ============
                    this.ReportViewer.ServerReport.SetParameters(reportParameter);
                    this.ReportViewer.ServerReport.Refresh();


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