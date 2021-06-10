namespace FintrakBanking.Common.Enum
{
    public enum LoanProductTypeEnum
    {
        TermLoan = 1,
        SelfLiquidating = 2,
        RevolvingLoan = 6,
        ContingentLiability = 9,
        LPO = 15,
        CFF =16,
        IDF = 17,
        CommercialLoan = 40,
        ForeignXRevolving = 41,
        SyndicatedTermLoan =42
    };

    public enum LoanSystemTypeEnum
    {
        TermDisbursedFacility = 1,
        OverdraftFacility = 2,
        ContingentLiability = 3,
        LineFacility = 4,
        ExternalFacility = 5,

    };

    public enum CollateralClassificationEnum
    {
        Tangible = 1,
        Intangible = 2,

    };
}