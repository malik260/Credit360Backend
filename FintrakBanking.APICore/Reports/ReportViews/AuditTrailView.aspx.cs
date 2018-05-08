using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Repositories.Admin;
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
            startDate.Text = Request.QueryString["startDate"];
            endDate.Text = Request.QueryString["endDate"];
            companyId.Text = Request.QueryString["companyId"];
            username.Text = Request.QueryString["username"];
            Id.Text = "2019";

            FinTrakBankingDocumentsContext context = new FinTrakBankingDocumentsContext();
            CompanyInformationRepository generalSetup = new CompanyInformationRepository(context);
          //  ReportParameter date = new ReportParameter("Path", generalSetup.GetCompanyImage());

         //   ReportViewer.LocalReport.SetParameters(new ReportParameter[] { date });
            ReportViewer.LocalReport.Refresh();
            ReportViewer.LocalReport.Refresh();
        }
    }
}