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
    public partial class ExpiredOverdraftLoans : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
              //  ReportViewer.LocalReport.Refresh();
            }
        }
    }
}