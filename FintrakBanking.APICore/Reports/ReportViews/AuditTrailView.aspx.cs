using FintrakBanking.Interfaces.Setups.General;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
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

                this.ReportViewer.LocalReport.EnableExternalImages = true;
                ReportParameter logo = new ReportParameter("logoPath", @"file:///C:\Users\uuser\Desktop\Fintrak\Credit 360\api\FintrakBankingAPI462\FintrakBanking.APICore\Content\Icons\firstbank-logo.jpg");
                ReportViewer.LocalReport.SetParameters(logo);
                ReportViewer.LocalReport.Refresh();
            }
            
        }
    }
}