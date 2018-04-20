namespace FintrakBanking.Common.Enum
{
    public enum LoanPrudentialStatusEnum
    {
        Performing = 1,
        WatchList = 2,
        Substandard = 3,
        Doubtful = 4,
        Lost = 5
    }





    public enum OverrideEnum
    {
        BlackbookOverride = 1,
        CAMSOLOverride = 2,
        SectorialLimitOverride = 3,
        ObligorLimitOverride = 4,
        RelationshipManagerLimitOverride = 5,
        NegativeCRMSOverride = 6,
        NegativeXDSOverride = 7,
        NegativeCRCOverride = 8,
        ConsessionaryDebtWriteOffOverride = 9,
        UpfrontFeeOverride = 10
    }
}