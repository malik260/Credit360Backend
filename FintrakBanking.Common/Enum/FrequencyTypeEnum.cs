namespace FintrakBanking.Common.Enum
{
    public enum FrequencyTypeEnum
    {
        Yearly = 1, TwiceYearly = 2, Quarterly = 3, SixTimesYearly = 4, Monthly = 5,
        TwiceMonthly = 6, Weekly = 7, Daily = 8, ThriceYearly = 9,
        EOD = 22, EOM = 23, EOY = 24, OneOff = 21, Transaction = 25
    };

    public enum TenorMode
    {
        Monthly = 1,
        Daily = 2,
        Yearly = 3
    }

    public enum AlertFrequencyEnum
    {
        HOURLY = 1,
        DAILY = 2,
        DAYCOUNT = 3,
        DATE = 4,
        WEEKLY = 5,
        BIWEEKLY = 6,
        MONTHLY = 7,
        BIMONTHLY = 8,
        QUARTERLY = 9,
        ANNUALY = 10,
        BIANNUAL = 11,
        CONDITIONONLY = 12,
        EVENT = 13,
    }
}

//1	Yearly
//2	Twice-Yearly
//3	Quarterly
//4	Six- times-Yearly
//5	Monthly
//6	Twice-Monthly
//7	Weekly
//8	Daily
//9	Thrice-Yearly



