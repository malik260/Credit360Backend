namespace FintrakBanking.Common.Enum
{
    public enum LoanScheduleTypeEnum
    {
        Annuity = 1, ReducingBalance = 2, BulletPayment = 3, AnnuityWithScheduledRepayment = 4, IrregularSchedule = 5, ConstantPrincipalAndInterest = 6,
        BallonPayment = 7
    };

    public enum LoanIrregularSchedulePaymentTypeEnum
    {
        PrincipalAndInterest = 1, PrincipalOnly = 2, InterestOnly = 3
    };

    public enum FullAndFinalStatusEnum
    {
        None = 1, OnGoing = 2, Cancelled = 3, Completed = 4
    };

    public enum RecoveryStatusEnum
    {
        None = 1, OnGoing = 2, Cancelled = 3, Completed = 4
    };

}

//ScheduleTypeId ScheduleTypeName    ScheduleCategoryId
//1	Annuity	1
//2	Reducing Balance    1
//3	Bullet Payment  1
//4	Annuity with Scheduled Repayment    1
//5	Irregular Schedule  2
//6	Constant Principal and Interest 1