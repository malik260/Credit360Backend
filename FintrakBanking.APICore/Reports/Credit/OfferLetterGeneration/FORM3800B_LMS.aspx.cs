using FintrakBanking.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.Credit.OfferLetterGeneration
{
    public partial class FORM3800B_LMS : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string inputDateInfo = Request.QueryString["key1"];
                    string inputHashValue = Request.QueryString["key2"];
                    applicationRefNumber.Text = Request.QueryString["applicationRefNumber"];

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

                    if (dateDifference.Seconds > 100)
                    {
                        this.offerLetterReport.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                        this.offerLetterReport.LocalReport.Refresh();
                        return;
                    }

                    //FinTrakBankingContext context = new FinTrakBankingContext();
                    //GeneralSetupRepository generalSetup = new GeneralSetupRepository(context);
                    //ReportParameter date = new ReportParameter("currentDate", generalSetup.GetApplicationDate().ToString("dd/MM/yyyy"));

                    //offerLetterReport.LocalReport.SetParameters(new ReportParameter[] { date });
                    offerLetterReport.LocalReport.Refresh();

                    //            Warning[] warnings;
                    //            string[] streamids;
                    //            string mimeType;
                    //            string encoding;
                    //            string filenameExtension;

                    //            byte[] bytes = offerLetterReport.LocalReport.Render(
                    //"PDF", null, out mimeType, out encoding, out filenameExtension,
                    //out streamids, out warnings);

                    //            using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
                    //            {
                    //                fs.Write(bytes, 0, bytes.Length);
                    //            }

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