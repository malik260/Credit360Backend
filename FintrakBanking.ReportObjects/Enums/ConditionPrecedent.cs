using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.Enums
{
    public enum ConditionPrecedent
    {
        [Description("Credit Approval")]
        CreditApproval,

        [Description("MPRSA")]
        MPRSA,

        [Description("MPRSA Refinancing Supplement")]
        MPRSARefinancingSupplement,

        [Description("Side Agreement")]
        SideAgreement,

        [Description("Board Resolution")]
        BoardResolution,

        [Description("Security Deed")]
        SecurityDeed,

        [Description("Insurance Policy for each Mortage Property - Property and Fire")]
        InsurancePolicyPropertyAndFire,

        [Description("Insurance Policy for each Mortage Property - Life and Disability")]
        InsurancePolicyLifeAndDisability,

        [Description("Property valuation report in respect of each Mortgage Property")]
        PropertyValuationReport,

        [Description("Rating report of the Bank")]
        BankRatingReport,

        [Description("Rating report of the Portfolio")]
        PortfolioRatingReport,

        [Description("Security/Title documents for each underlying mortgage property")]
        TitleDocuments,

        [Description("Loan Agreement for each underlying mortgage property")]
        LoanAgreement,

        [Description("Offer letter for each underlying mortgage property")]
        OfferLetter,

        [Description("Report on Bank's deal flow")]
        DealFlowReport,

        [Description("Portfolio activity report")]
        PortfolioActivityReport,

        [Description("Repayment schedules")]
        RepaymentSchedules,

        [Description("Exception to UUS")]
        ExceptionToUUS,

        [Description("NIBBS Mandate")]
        NIBBSMandate
    }
}
