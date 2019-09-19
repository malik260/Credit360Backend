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
                    //DateTime RunDate = DateTime.ParseExact(Request.QueryString["runDate"], "dd-MM-yyyy", null);
                    //string Level = Request.QueryString["level"];
                    //string MisCode = Request.QueryString["misCode"];
                    //string ExposureType = Request.QueryString["exposureType"];
                    //string DivisionName = Request.QueryString["divisionName"];
                    //string GroupName = Request.QueryString["groupName"];
                    //string BranchName = Request.QueryString["branchName"];
                    //string SectorName = Request.QueryString["sectorName"];

                    //string inputDateInfo = Request.QueryString["key1"];
                    //string inputHashValue = Request.QueryString["key2"];

                    HashHelper hash = new HashHelper();

                    //DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    //var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    //if (inputHashValue != incomingDateHash)
                    //{
                    //    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    //    this.ReportViewer.LocalReport.Refresh();
                    //    return;
                    //}

                    var currentDate = DateTime.Now;

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


                    ReportViewer.ServerReport.ReportServerUrl = new Uri(reportServerUrl);
                    ReportViewer.ServerReport.ReportServerCredentials = new ReportServerCredentials(userName, password, domain);
                    ReportViewer.ServerReport.ReportPath = reportPath + "Risk Asset"; // string.Format(reportPath, "Risk Asset");

                    ReportViewer.ProcessingMode = ProcessingMode.Remote;
                    ReportViewer.ShowCredentialPrompts = false;

                    ReportParameter[] reportParameter = new ReportParameter[9];
                    reportParameter[0] = new ReportParameter("MisCode", "bnk"); //MisCode
                    reportParameter[1] = new ReportParameter("Level", "0"); //Level
                    reportParameter[2] = new ReportParameter("ExposureType",  "Direct" );  //ExposureType
                    reportParameter[3] = new ReportParameter("DivisionName", "BUSINESS BANKING DIVISION"); //DivisionName
                    reportParameter[4] = new ReportParameter("GroupName", "BUSINESS BANKING East"); //GroupName
                    reportParameter[5] = new ReportParameter("BranchName", "BBD Branch_Business Banking Team (Aba-Aziukwu)"); //BranchName
                    reportParameter[6] = new ReportParameter("SectorName", "General"); //SectorName
                    reportParameter[7] = new ReportParameter("RunDate", "06/30/2019"); //RunDate.ToString()
                    reportParameter[8] = new ReportParameter("RegionName", "BUSINESS BANKING Abia Zone"); //SectorName

                    //==== NOTE: for report on server, use the below ============
                    ReportViewer.ServerReport.SetParameters(reportParameter);
                    ReportViewer.ServerReport.Refresh();


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