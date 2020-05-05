using FintrakBanking.Common.Enum;
using FintrakBanking.ReportObjects.Credit;
using Microsoft.Reporting.WebForms;
using System;


namespace FintrakBanking.APICore.Reports.Credit.OfferLetterGeneration
{
    public partial class OfferLetter : System.Web.UI.Page
    {
       
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string applicationRefNumber = Request.QueryString["applicationRefNumber"];
                    GenerateOutPutDocument(applicationRefNumber);

                }
                catch(Exception ex)
                {
                    //throw new Exception(ex.Message);
                    this.offerLetterReport.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
                    this.offerLetterReport.LocalReport.Refresh();
                    return;
                }
            }
        }

        void GenerateOutPutDocument(string applicationRefNumber)
        {
            OfferLetterInfo offerLetter = new OfferLetterInfo();

            var offerLetterDetails = offerLetter.GenerateOfferLetter(applicationRefNumber);
            this.offerLetterReport.LocalReport.DataSources.Clear();
            ReportDataSource dsOfferLetterDetails = new ReportDataSource();
            dsOfferLetterDetails.Value = offerLetterDetails;
            dsOfferLetterDetails.Name = "OfferLetterDetails";

            var loanApplicationDetail = offerLetter.GetLoanApplicationDetail(applicationRefNumber);
            ReportDataSource dsLoanApplicationDetail = new ReportDataSource();
            dsLoanApplicationDetail.Value = loanApplicationDetail;
            dsLoanApplicationDetail.Name = "OfferLetterLoanDetail";

            var conditionPrecident = offerLetter.GetLoanApplicationConditionPrecident(applicationRefNumber);
            ReportDataSource dsConditionPrecident = new ReportDataSource();
            dsConditionPrecident.Value = conditionPrecident;
            dsConditionPrecident.Name = "OfferLetterConditionPrecident";

            var conditionSubsequent = offerLetter.GetLoanApplicationConditionSubsequent(applicationRefNumber);
            ReportDataSource dsConditionSubsequent = new ReportDataSource();
            dsConditionSubsequent.Value = conditionSubsequent;
            dsConditionSubsequent.Name = "OfferLetterConditionSubsequent";

            var fee = offerLetter.GetLoanApplicationFee(applicationRefNumber);
            ReportDataSource dsFee = new ReportDataSource();
            dsFee.Value = fee;
            dsFee.Name = "OfferLetterFee";

            var signatory = offerLetter.GetLoanApplicationSignatory(applicationRefNumber);
            ReportDataSource dsSignatory = new ReportDataSource();
            dsSignatory.Value = signatory;
            dsSignatory.Name = "OfferLetterSignatory";

            var collateral = offerLetter.GetLoanCollateral(applicationRefNumber);
            ReportDataSource dsCollateral = new ReportDataSource();
            dsCollateral.Value = collateral;
            dsCollateral.Name = "OfferLetterCollateral";

            var generateOfferLetter = offerLetter.GenerateOfferLetter(applicationRefNumber);
            ReportDataSource dsGenerateOfferLetter = new ReportDataSource();
            dsGenerateOfferLetter.Value = generateOfferLetter;
            dsGenerateOfferLetter.Name = "OfferLetterBorrowerDetail";

            var getLeaseFacility = offerLetter.GetLeaseFacility(applicationRefNumber);
            ReportDataSource dsGetLeaseFacility = new ReportDataSource();
            dsGetLeaseFacility.Value = getLeaseFacility;
            dsGetLeaseFacility.Name = "LeaseFacility";

            var getDynamics = offerLetter.Los_ConditionDynamics(applicationRefNumber);
            ReportDataSource dsGetDynamics = new ReportDataSource();
            dsGetDynamics.Value = getDynamics;
            dsGetDynamics.Name = "OfferLetterTransactionDynamics";

            offerLetterReport.LocalReport.DataSources.Add(dsOfferLetterDetails);
            offerLetterReport.LocalReport.DataSources.Add(dsLoanApplicationDetail);
            offerLetterReport.LocalReport.DataSources.Add(dsConditionPrecident);
            offerLetterReport.LocalReport.DataSources.Add(dsConditionSubsequent);
            offerLetterReport.LocalReport.DataSources.Add(dsCollateral);
            offerLetterReport.LocalReport.DataSources.Add(dsFee);
            offerLetterReport.LocalReport.DataSources.Add(dsSignatory);
            offerLetterReport.LocalReport.DataSources.Add(dsCollateral);
            offerLetterReport.LocalReport.DataSources.Add(dsGenerateOfferLetter);
            offerLetterReport.LocalReport.DataSources.Add(dsGetLeaseFacility);
            offerLetterReport.LocalReport.DataSources.Add(dsGetDynamics);

            var reportLink = string.Empty;

            foreach(var x in loanApplicationDetail)
            {
                if (x.productClassId == 41 && x.productTypeId != 102)
                {
                    reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_ImportFinance.rdlc");
                } else if (x.productClassId == 33 && x.productTypeId == 102)
                {
                    reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_LeaseFacility.rdlc");
                }else if (x.productClassId == 33 && x.productTypeId != 102)
                {
                    reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter.rdlc");
                }
                else
                {
                    reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter.rdlc");
                }
               
                //switch (x.productClassId)
                //{
                //    case (int)ProductClassEnum.ImportFinanceFacilities:
                //        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_ImportFinance.rdlc");
                //        break;
                //    case (int)ProductClassEnum.EmergingBusiness:
                //        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_LeaseFacility.rdlc");
                //        break;
                //    case (int)ProductClassEnum.BondAndGuarantees:
                //        reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter_OtherGuarantee.rdlc");
                //        break;

                //    default: reportLink = Server.MapPath("~/Reports/Credit/OfferLetterGeneration/OfferLetter.rdlc");
                //        break;
                //}
            }

            this.offerLetterReport.LocalReport.ReportPath = reportLink;
            this.offerLetterReport.LocalReport.Refresh();
        }


    }
}