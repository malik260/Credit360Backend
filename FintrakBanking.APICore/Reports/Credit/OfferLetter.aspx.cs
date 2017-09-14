using FintrakBanking.Entities.Models;
//using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.Credit
{
    public partial class OfferLetter : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                //ReportParameter date = new ReportParameter("currentDate", DateTime.Now.ToString());

                //offerLetter.LocalReport.SetParameters(new ReportParameter[] { date });
                //offerLetter.LocalReport.Refresh();
            }
        }
    }
}