namespace FintrakBanking.Common.Enum
{
    public enum LoanPrudentialStatusEnum
    {
        WatchList = 1,
        Performing = 2,
        Substandard = 3,
        Doubtful = 4,
        Lost = 5
    }

    public enum PrudentialGuidelineTypeEnum
    {
        Performing = 1,
        NonPerforming = 2,
    }



    public enum OverrideEnum
    {
        BlackbookOverride = 1,
        CAMSOLOverride = 2,
        TakeFeeAtDisbursement = 3,
        ObligorLimitOverride = 4,
        RelationshipManagerLimitOverride = 5,
        NegativeCRMSOverride = 6,
        NegativeXDSOverride = 7,
        NegativeCRCOverride = 8,
        ConsessionaryDebtWriteOffOverride = 9,
        UpfrontFeeOverride = 10,
        SectorialLimitOverride = 11,
    }
}