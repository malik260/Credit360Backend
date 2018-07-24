 
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
    public partial class PostedFinancialTransactions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string imagePath = new Uri(Server.MapPath("~/Content/icons/firstbank-logo.jpg")).AbsoluteUri;
                //string tmpPath = @"image\firstbank-logo.jpg";
                //string a = Path.GetFullPath(tmpPath);
                //string ProjectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)));
                //var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                //var imagePath = Path.Combine(outPutDirectory, tmpPath);
               // var a = @"C:\Users\uuser\Desktop\Fintrak\Credit360\API\FintrakBankingAPI462\FintrakBanking.APICore\Content\icons\firstbank-logo.jpg";
                ReportParameter data = new ReportParameter("selectedDate", DateTime.Now.ToString());
                StartDate.Text = Request.QueryString["StartDate"];
                EndDate.Text = Request.QueryString["EndDate"];
                PostedByStaffId.Text = Request.QueryString["PostedByStaffId"];
                glAccountId.Text = Request.QueryString["glAccountId"];
                branchId.Text = Request.QueryString["branchId"];
                companyId.Text = Request.QueryString["companyId"];

                ReportParameter sDate = new ReportParameter("StartDate", StartDate.Text);
                ReportParameter eDate = new ReportParameter("EndDate", EndDate.Text);
                ReportParameter logoPath = new ReportParameter("logo", imagePath);

                this.ReportViewer.LocalReport.EnableExternalImages = true;
                ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate, logoPath });
                ReportViewer.LocalReport.Refresh();

            }
        }
    }
}