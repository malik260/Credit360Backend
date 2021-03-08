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
    public partial class DisbursedFacilityCollateralReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];
                    //DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                    //DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);



                    int classification = int.Parse(Request.QueryString["classification"]);
                    int productId = int.Parse(Request.QueryString["productId"]);

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

                    if (dateDifference.Seconds > 30)
                    {
                        this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.ReportViewer.LocalReport.Refresh();
                        return;
                    }

                    LoanReportObjects DisbursedFacilityCollateralReport = new LoanReportObjects();
                    var data = DisbursedFacilityCollateralReport.DisbursedFacilityCollateralReport(classification, productId);

                    this.ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = data;
                    reportDataSource.Name = "DisbursedFacilityDataSet";
                    this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/DisbursedFacilityCollateralReport.rdlc");

                    string exportOption = "PDF";
                    RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                    if (extension != null)
                    {
                        System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        fieldInfo.SetValue(extension, false);
                    }

                    ReportViewer.LocalReport.Refresh();
                }
                catch (Exception ex)
                {
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.ReportViewer.LocalReport.Refresh();
                    return;
                }
            }
            /* if (!IsPostBack)
             {
                 try
                 {
                     DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                     DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
                     int companyId = Int32.Parse(Request.QueryString["companyId"]);
                     int status = Int32.Parse(Request.QueryString["status"]);
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

                     if (dateDifference.Seconds > 30)
                     {
                         this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                         this.ReportViewer.LocalReport.Refresh();
                         return;
                     }
                     LoanReportObjects DisbursedFacilityCollateralReport = new LoanReportObjects();
                     var data = DisbursedFacilityCollateralReport.DisbursedFacilityCollateralReport(startDate, endDate, status, companyId);

                     this.ReportViewer.LocalReport.DataSources.Clear();
                     ReportDataSource reportDataSource = new ReportDataSource();
                     reportDataSource.Value = data;
                     reportDataSource.Name = "DisbursedFacilityDataSet";

                     ReportParameter sDate = new ReportParameter("startDate", startDate.ToString());
                     ReportParameter eDate = new ReportParameter("endDate", endDate.ToString());

                     this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                     this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/DisbursedFacilityCollateralReport.rdlc");
                     ReportViewer.LocalReport.Refresh();
                 }
                 catch (Exception ex)
                 {
                     this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                     this.ReportViewer.LocalReport.Refresh();
                     return;
                 }
             } */
        }
    }
}