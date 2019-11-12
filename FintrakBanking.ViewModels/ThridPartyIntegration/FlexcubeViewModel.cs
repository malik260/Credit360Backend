using System;
namespace FintrakBanking.ViewModels.Flexcube
{

    public class FlexcubeLienViewModel : GeneralEntity
    {
        public string p_account_no { get; set; }

        public string p_collateral_code { get; set; }

        public string p_collateral_value { get; set; }

        public string p_start_date { get; set; }
        public string p_end_date { get; set; }
        public string p_collateral_id { get; set; }

        public string p_contract_ref_no { get; set; }

        public string p_collateral_contribution { get; set; }

        public string p_branch_code { get; set; }

        public string p_channel_code { get; set; }

    }

    public class FlexcubeCreateOverdraftViewModel //: GeneralEntity
    {
        public string p_account_no { get; set; }

        public string p_collateral_code { get; set; }

        public string p_collateral_value { get; set; }

        public string p_start_date { get; set; }
        public string p_end_date { get; set; }
        public string p_channel_code { get; set; }
    }

    public class FlexcubeCreateOverdraftWithoutLienViewModel : GeneralEntity
    {
        public string p_account_no { get; set; }

        public string p_collateral_code { get; set; }

        public string p_collateral_value { get; set; }

        public string p_start_date { get; set; }
        public string p_end_date { get; set; }
        public string p_channel_code { get; set; }
    }

    public class FlexcubeCreateFacilityViewModel : GeneralEntity
    {
        public string p_account_no { get; set; }

        public string p_liab_no { get; set; }

        public string p_line_code { get; set; }

        public string p_line_serial { get; set; }
        public string p_start_date { get; set; }
        public string p_expiry_date { get; set; }

        public string p_limit_amount { get; set; }
        public string p_collateral_amount { get; set; }
        public string p_facility_description { get; set; }

        public string p_effective_date { get; set; }
        public string p_collateral_code { get; set; }
        public string p_interest_rate { get; set; }
        public string p_channel_code { get; set; }
        public string sourceReferenceNumber { get; set; }
    }


    public class FlexcubeCreateLoanAccountViewModel : GeneralEntity
    {
        public string source { get; set; }

        public string app_user_id { get; set; }

        public string app_branch_code { get; set; }

        public string product_code { get; set; }
        public string product_cat { get; set; }
        public string user_refno { get; set; }

        public string book_date { get; set; }
        public string value_date { get; set; }
        public string maturity_date { get; set; }
        public string effective_date { get; set; }

        public string amount_financed { get; set; }
        public string account_no { get; set; }
        public string product_desc { get; set; }
        public string no_of_financials { get; set; }

        public string due_dateson { get; set; }
        public string inst_date { get; set; }
        public string advisory_fee { get; set; }
        public string anniversary_fee { get; set; }
        public string creditlife_rate { get; set; }
        public string mgt_rate { get; set; }

        public string appraisal_fee { get; set; }
        public string committment_fee { get; set; }
        public string creditlife_fee { get; set; }
        public string in_odchrg_fee { get; set; }

        public string interest_rate { get; set; }
        public string mgt_fee { get; set; }
        public string penal_charge { get; set; }
        public string prn_odchrg_fee { get; set; }

        public string processing_fee { get; set; }
        public string renann_fee { get; set; }
        public string tax_rate { get; set; }
        public string vehicle_ins { get; set; }

        public string vehicle_value { get; set; }
        public string crms_ref_number { get; set; }
        public string comp_mis8 { get; set; }
        public string freq_unit { get; set; }

        public string disbursement_type { get; set; }

        public string sourceReferenceNumber { get; set; }
    }

    public class LoanCreationViewModel
    {
        public string source { get; set; }
        public string app_user_id { get; set; }
        public string app_branch_code { get; set; }
        public string product_code { get; set; }
        public string product_cat { get; set; }
        public string user_refno { get; set; }
        public string book_date { get; set; }
        public string value_date { get; set; }
        public string maturity_date { get; set; }
        public string amount_financed { get; set; }
        public string account_no { get; set; }
        public string product_desc { get; set; }
        public string effective_date { get; set; }
        public string creditlife_rate { get; set; }
        public string interest_rate { get; set; }
        public string mgt_rate { get; set; }
        public string tax_rate { get; set; }
        public string first_repayment_date { get; set; }
        public string sourceReferenceNumber { get; set; }
        public int operationId { get; set; }

        public string no_of_financials { get; set; }
        public string due_dateson { get; set; }
        public string inst_date { get; set; }
        public string advisory_fee { get; set; }
        public string anniversary_fee { get; set; }
        public string appraisal_fee { get; set; }
        public string committment_fee { get; set; }
        public string creditlife_fee { get; set; }
        public string in_odchrg_fee { get; set; }
        public string mgt_fee { get; set; }
        public string penal_charge { get; set; }
        public string prn_odchrg_fee { get; set; }
        public string processing_fee { get; set; }
        public string renann_fee { get; set; }
        public string vehicle_ins { get; set; }
        public string vehicle_value { get; set; }
        public string crms_ref_number { get; set; }
        public string comp_mis8 { get; set; }
        public string freq_unit { get; set; }
        public string disbursement_type { get; set; }

    }
}

