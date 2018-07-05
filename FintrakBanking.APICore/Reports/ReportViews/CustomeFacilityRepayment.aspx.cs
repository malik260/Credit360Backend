using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class CustomeFacilityRepayment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate.Text = Request.QueryString["startDate"];
                endDate.Text = Request.QueryString["endDate"];
                companyId.Text = Request.QueryString["companyId"];
                valueCode.Text = Request.QueryString["valueCode"];

                string tmpPath = @"Content\Icons\firstbank-logo.jpg";
                string a = Path.GetFullPath(tmpPath);
                string ProjectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)));
                var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                var imagePath = Path.Combine(outPutDirectory, tmpPath);

                this.ReportViewer.LocalReport.EnableExternalImages = true;
                ReportParameter logo = new ReportParameter("Path", imagePath);
                ReportParameter sDate = new ReportParameter("startDate", startDate.Text);
                ReportParameter eDate = new ReportParameter("endDate", endDate.Text);
              //  ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate });
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}