namespace FintrakBanking.Common.Enum
{
    public enum DayCountConventionEnum { US_NASD_30_360 = 0, Actual_Actual = 1 , Actual_360 = 2, Actual_365 = 3, European_30_360 = 4,
                                          Isda_30_360 = 5, No_Leap_Year_365 = 7, No_Leap_Year_360 = 8, Actual_364 = 9};

}

//DayCountId DayCountName    DaysInAYear
//0	US(NASD) 30/360	360
//1	Actual/Actual	-1
//2	Actual/360	360
//3	Actual/365	365
//4	European 30/360	360
//5	30/360 ISDA	360
//7	No Leap Year /365	365
//8	No Leap Year /360	360
//9	Actual/364	364

