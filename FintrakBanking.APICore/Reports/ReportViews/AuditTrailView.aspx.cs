using FintrakBanking.Interfaces.Setups.General;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class AuditTrail : System.Web.UI.Page
    {
     
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate.Text = Request.QueryString["startDate"];
                endDate.Text = Request.QueryString["endDate"];
                companyId.Text = Request.QueryString["companyId"];
                username.Text = Request.QueryString["username"];
                auditTypeId.Text = Request.QueryString["auditTypeId"];

                string tmpPath = @"Content\Icons\firstbank-logo.jpg";
                string a = Path.GetFullPath(tmpPath);
                string ProjectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)));
                var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                var imagePath = Path.Combine(outPutDirectory, tmpPath);

                this.ReportViewer.LocalReport.EnableExternalImages = true;
               // ReportParameter logo = new ReportParameter("logoPath", imagePath);
               // ReportViewer.LocalReport.SetParameters(logo);
                ReportViewer.LocalReport.Refresh();
            }

        }
    }
}