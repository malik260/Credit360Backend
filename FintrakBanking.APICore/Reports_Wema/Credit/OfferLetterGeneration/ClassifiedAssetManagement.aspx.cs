using FintrakBanking.Common.Extensions;
using FintrakBanking.Entities.Models;
using FintrakBanking.ReportObjects.Credit;
using FintrakBanking.Repositories.Setups.General;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.Credit.OfferLetterGeneration
{
    public partial class ClassifiedAssetManagement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                     loanId.Text = Int32.Parse(Request.QueryString["loanId"]).ToString();
                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];

                    HashHelper hash = new HashHelper();

                    DateTime incomingDate = DateTime.ParseExact(inputDateInfo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

                    var incomingDateHash = hash.HashString(inputDateInfo).Replace("-", "");

                    if (inputHashValue != incomingDateHash)
                    {
                        this.offerLetterReport.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.offerLetterReport.LocalReport.Refresh();
                        return;
                    }

                    var currentDate = DateTime.Now;

                    var dateDifference = currentDate - incomingDate;

                    if (dateDifference.Seconds > 10)
                    {
                        this.offerLetterReport.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.offerLetterReport.LocalReport.Refresh();
                        return;
                    }

                    //string logo = Server.MapPath("~/Content/icons/firstbank.png");

                    ////LoanReportObjects loanRepo = new LoanReportObjects();
                    //var data = OfferLetterInfoLMSR.GetManagementPosition(loanId);

                    //this.offerLetterReport.LocalReport.DataSources.Clear();
                    //ReportDataSource reportDataSource = new ReportDataSource();
                    //reportDataSource.Value = data;
                    //reportDataSource.Name = "WriteOff";

                    //this.offerLetterReport.LocalReport.DataSources.Add(reportDataSource);
                    //this.offerLetterReport.LocalReport.ReportPath = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/ClassifiedAssetManagement.rdlc");
                    this.offerLetterReport.LocalReport.Refresh();
                }

                catch (Exception ex)
                {
                    this.offerLetterReport.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.offerLetterReport.LocalReport.Refresh();
                    return;
                }
            }
        }
    }
}