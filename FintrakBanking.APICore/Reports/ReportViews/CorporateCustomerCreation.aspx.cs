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
    public partial class CorporateCustomerCreation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                try
                {
                    DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                    DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
                    //string referenceNumber = Request.QueryString["customerId"];
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

                    //if (dateDifference.Seconds > 30)
                    //{
                    //    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    //    this.ReportViewer.LocalReport.Refresh();
                    //    return;
                    //}

                    LoanReportObjects reportObjects = new LoanReportObjects();
                    var data = reportObjects.CorporateCustomerCreation(startDate, endDate);

                    string exportOption = "PDF";
                    RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                    if (extension != null)
                    {
                        System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        fieldInfo.SetValue(extension, false);
                    }

                    this.ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource reportDataSource = new ReportDataSource();
                    reportDataSource.Value = data;
                    reportDataSource.Name = "DataSet1";

                    this.ReportViewer.LocalReport.DataSources.Add(reportDataSource);
                    this.ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/CorporateCustomerCreation.rdlc");
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