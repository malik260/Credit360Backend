using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.Credit.Monitoring
{
    public partial class CovenantsApproachingDueDate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate.Text = Request.QueryString["startDate"];
                endDate.Text = Request.QueryString["endDate"];
                companyId.Text = Request.QueryString["companyId"];
                branchId.Text = Request.QueryString["branchId"];
                loanRefNo.Text = Request.QueryString["loanRefNo"];
                productClassId.Text = Request.QueryString["productClassId"];
                staffId.Text = Request.QueryString["staffId"];

                ReportParameter sDate = new ReportParameter("startDate", startDate.Text);
                ReportParameter eDate = new ReportParameter("endDate", endDate.Text);

                FinTrakBankingContext context = new FinTrakBankingContext();
                GeneralSetupRepository generalSetup = new GeneralSetupRepository(context);
                ReportParameter date = new ReportParameter("currentDate", generalSetup.GetApplicationDate().ToShortDateString());

              //  covDueDateRv.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate });
                covDueDateRv.LocalReport.Refresh();


            }
        }
    }
}