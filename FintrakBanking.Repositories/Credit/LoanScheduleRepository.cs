using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using NodaTime;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Credit
{
    using wct = XLeratorDLL_financial.XLeratorDLL_financial;
    using FinancialTypes = XLeratorDLL_financial.FinancialTypes;

     
    public class LoanScheduleRepository: ILoanScheduleRepository
    {
        private FinTrakBankingContext context;

        public LoanScheduleRepository(FinTrakBankingContext _context)
        {
            this.context = _context;            
        }

        public int CalculateNumberOfInstallments(TenorModeEnum tenorModeId, short frequencyTypeId, int tenor)
        {
            double totalTenor = 0;

            if (tenorModeId == TenorModeEnum.Days)
                totalTenor = tenor / 365; //365 days in a year
            else if (tenorModeId == TenorModeEnum.Months)
                totalTenor = tenor / 12; //12 = months in a year
            else if (tenorModeId == TenorModeEnum.Years)
                totalTenor = tenor;

            if (frequencyTypeId == 10 || frequencyTypeId == 11) // 10 = end of period and 11 = now
                return 1;

            var frequencyValue = context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == frequencyTypeId).Value;

            var installments = totalTenor * frequencyValue;

            return (int)installments;
        }

        public int CalculateNumberOfInstallments(DateTime firstPaymentDate, DateTime maturityDate, FrequencyTypeEnum frequencyType)
        {
            var startdate = LocalDateTime.FromDateTime(firstPaymentDate);
            var endDate = LocalDateTime.FromDateTime(maturityDate);

            //Period period = Period.Between(startdate, endDate, PeriodUnits.Months);

            double numberOfpayments = 0;

            if (frequencyType == FrequencyTypeEnum.Daily)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Days).Days;
            else if (frequencyType == FrequencyTypeEnum.Monthly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months;
            else if (frequencyType == FrequencyTypeEnum.Quarterly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 3.0;
            else if (frequencyType == FrequencyTypeEnum.SixTimesYearly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 6.0;
            else if (frequencyType == FrequencyTypeEnum.ThriceYearly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 4.0;
            else if (frequencyType == FrequencyTypeEnum.TwiceMonthly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months * 2.0;
            else if (frequencyType == FrequencyTypeEnum.TwiceYearly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 6.0;
            else if (frequencyType == FrequencyTypeEnum.Weekly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Weeks).Weeks;
            else if (frequencyType == FrequencyTypeEnum.Yearly)
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Years).Years;


            ////if (tenorModeId == TenorModeEnum.Days)
            //totalTenor = tenor / daysInAYear; //365 days in a year

            //var frequencyValue = context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == (short) frequencyType).Value;

            //var installments = totalTenor * frequencyValue;

            return Convert.ToInt32(numberOfpayments + 1);
        }


        public DateTime CalculateFirstPayDate(DateTime effectiveDate, short frequencyTypeId)
        {
            var frequencyValue = context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == frequencyTypeId).Value;

            DateTime output = effectiveDate.AddMonths(12 / (int)frequencyValue);
            return output;
        }

        public IEnumerable<LookupViewModel> GetAllLoanScheduleCategory()
        {
            return (from data in context.tbl_Loan_Schedule_Category
                    select new LookupViewModel()
                    {
                        lookupId = data.ScheduleCategoryId,
                        lookupName = data.ScheduleCategoryName
                    });
        }

        public IEnumerable<LookupViewModel> GetAllLoanScheduleType()
        {
            return (from data in context.tbl_Loan_Schedule_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.ScheduleTypeId,
                        lookupName = data.ScheduleTypeName,
                        lookupTypeId = data.ScheduleCategoryId,
                        lookupTypeName = data.tbl_Loan_Schedule_Category.ScheduleCategoryName
                    });
        }


        public IEnumerable<LookupViewModel> GetLoanScheduleTypeByCategory(short categoryId)
        {
            return (from data in context.tbl_Loan_Schedule_Type
                    where data.ScheduleCategoryId == categoryId
                    select new LookupViewModel()
                    {
                        lookupId = data.ScheduleTypeId,
                        lookupName = data.ScheduleTypeName,
                        lookupTypeId = data.ScheduleCategoryId,
                        lookupTypeName = data.tbl_Loan_Schedule_Category.ScheduleCategoryName
                    });
        }

        public List<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {            

            List<LoanPaymentSchedulePeriodicViewModel> output = null; // new List<LoanPaymentSchedulePeriodicViewModel>();
            LoanScheduleTypeEnum scheduleMethod = (LoanScheduleTypeEnum)loanInput.scheduleMethodId;

            if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                output = GenerateIrregularPeriodicScheduleWithAmortisedCost(loanInput).ToList();
            else if (scheduleMethod == LoanScheduleTypeEnum.Annuity)
            {
                if (loanInput.interestFirstpaymentDate == loanInput.principalFirstpaymentDate && loanInput.interestFrequency == loanInput.principalFrequency)
                    output = GenerateNormalAnnuityPeriodicSchedule(loanInput);
                else
                    output = GenerateMoratoriumAnnuityPeriodicSchedule(loanInput);
            }
            else if (scheduleMethod == LoanScheduleTypeEnum.ReducingBalance)
                output = GenerateReducingBalancePeriodicSchedule(loanInput);
            else if (scheduleMethod == LoanScheduleTypeEnum.BulletPayment)
                output = GenerateBulletPeriodicScheduleWithAmortisedCost(loanInput);
            else if (scheduleMethod == LoanScheduleTypeEnum.ConstantPrincipalAndInterest)
                output = GenerateConstantPrincipalAndInterestPeriodicScheduleWithAmortisedCost(loanInput);
            else if (scheduleMethod == LoanScheduleTypeEnum.BallonPayment)
                output = GenerateBallonPeriodicSchedule(loanInput);

            return output;
        }

        private int GetDaysInAYear(DayCountConventionEnum dayCountId)
        {
            if (dayCountId == DayCountConventionEnum.Actual_Actual)
            {
                var currentDate = DateTime.Now;
                var firstDate = new DateTime(currentDate.Year, 1, 1); //  DateTime.ParseExact(user, "MM-dd-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                var lastdate = new DateTime(currentDate.Year, 12, 31);
                var difference = (lastdate - firstDate).TotalDays;

                return Convert.ToInt32(difference);
            }

            var value = context.tbl_Day_Count.FirstOrDefault(x => x.DayCountId == (short)dayCountId).DaysInAYear;

            return value;
        }

        private double CalculateIRR(LoanPaymentScheduleInputViewModel loanInput, List<LoanPaymentSchedulePeriodicViewModel> cashflow, int numberOfPayments, int numberOfPaymentsInAYear,
                                    int daysInAYear, Double FV, string interestRule, DateTime maturityDate)
        {
            double output = 0;


            if (loanInput.integralFeeAmount == 0)
                output = loanInput.interestRate / 100;
            else if (numberOfPayments < 2 && loanInput.interestRate > 0)
            {
                //var amounts = paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
                //var dates = paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.paymentDate).ToList();

                List<double> cashflowAmounts = new List<double>();
                List<int> paymentNumbers = new List<int>();

                cashflowAmounts.Add((loanInput.principalAmount - loanInput.integralFeeAmount) * -1);
                paymentNumbers.Add(0);

                var counter = 0;
                foreach (var payment in cashflow)
                {
                    if (counter > 0)
                    {
                        cashflowAmounts.Add(payment.periodPaymentAmount);
                        paymentNumbers.Add(payment.paymentNumber);
                    }

                    counter += 1;
                }

                output = wct.IRR(cashflowAmounts, paymentNumbers, wct.NULL_DOUBLE);
            }
            else if (numberOfPayments > 1 && loanInput.interestRate > 0)
            {
                var pmt = wct.LPMT(loanInput.principalAmount, loanInput.effectiveDate, loanInput.interestRate / 100.0, loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear,
                                   daysInAYear, FV, interestRule);

                output = wct.LRATE(loanInput.principalAmount - loanInput.integralFeeAmount, loanInput.effectiveDate, pmt, loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear,
                                      daysInAYear, 0, interestRule, wct.NULL_DOUBLE);

                //  SELECT @IRR = wct.LRATE(
                // @la - @fa--PV
                //, @sd--Loan Date
                //,[wct].[LPMT](@la, @sd, cast(cast(@rt as float) / cast(100 as float) as float), @fpd, @np, @rfqy, @NDY, NULL, 'U')--PMT
                //, @fpd--FirstPayment Date
                //, @np--Number ofPaymens
                //, @rfqy--Payments peryear
                //, @NDY--Days in Year
                //, 0--FV
                //, 'U'--InterestRule
                //, NULL--Guess
                //)

            }
            else if (loanInput.interestRate <= 0)
                output = 0;

            if (loanInput.interestFirstpaymentDate == maturityDate)
            {
                Period period = Period.Between(LocalDateTime.FromDateTime(loanInput.interestFirstpaymentDate), LocalDateTime.FromDateTime(maturityDate));

                output = output * (365 / period.Days);
            }

            return output * 100;
        }


        /// <summary>
        /// USE FOR NORMAL ANNUITY SCHEDULE AS SHOWN WHERE INTEREST AND PRINCIPAL DROPS THE SAME DAY
        /// </summary>
        /// <param name="loanInput"></param>
        /// <returns></returns>         
        private List<LoanPaymentSchedulePeriodicViewModel> GenerateNormalAnnuityPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new Exception("Effective Date should be less than the maturity date");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);
            int numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value;

            Double FV;
            FinancialTypes.InterestRuleType IntRule;
            FinancialTypes.AMORTSCHED_table result;

            FV = wct.NULL_DOUBLE;
            IntRule = FinancialTypes.InterestRuleType.US;

            //result = wct.AMORTSCHED(PV, LoanDate, rate, FirstPayDate, NumPmts, Pmtpyr, DaysInYr, FV, IntRule);

            result = wct.AMORTSCHED(loanInput.principalAmount, loanInput.effectiveDate, (loanInput.interestRate / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);


            int counter = 0;
            foreach (DataRow row in result.Rows)
            {
                LoanPaymentSchedulePeriodicViewModel payment = new LoanPaymentSchedulePeriodicViewModel();
                payment.paymentNumber = Convert.ToInt32(row["num_pmt"]);
                payment.paymentDate = Convert.ToDateTime(row["date_pmt"]);
                payment.startPrincipalAmount = Convert.ToDouble(row["amt_prin_init"]);
                payment.periodPaymentAmount = Convert.ToDouble(row["amt_pmt"]);
                payment.periodInterestAmount = Convert.ToDouble(row["amt_int_pay"]);
                payment.periodPrincipalAmount = Convert.ToDouble(row["amt_prin_pay"]);
                payment.endPrincipalAmount = Convert.ToDouble(row["amt_prin_end"]);
                payment.interestRate = loanInput.interestRate / 100.0;

                output.Add(payment);

                counter += 1;
            }

            var maturityDate = output.Max(x => x.paymentDate); //loanInput.maturityDate

            var internalRateOfReturn = CalculateIRR(loanInput, output, numberOfPayments, numberOfPaymentsInAYear,
                                             daysInAYear, FV, "U", maturityDate);

            FinancialTypes.AMORTSCHED_table amortisedResult;
            amortisedResult = wct.AMORTSCHED(loanInput.principalAmount - loanInput.integralFeeAmount, loanInput.effectiveDate, (internalRateOfReturn / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            counter = 0;
            foreach (var payment in output)
            {
                payment.amortisedStartPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_init"]);
                payment.amortisedPeriodPaymentAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_pmt"]);
                payment.amortisedPeriodInterestAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_int_pay"]);
                payment.amortisedPeriodPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_pay"]);
                payment.amortisedEndPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_end"]);
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return output;

        }

        /// <summary>
        /// USE FOR ANNUITY SCHEDULE WHERE THERE IS A MORATORIUM 
        /// </summary>
        /// <param name="loanInput"></param>
        /// <returns></returns>         
        private List<LoanPaymentSchedulePeriodicViewModel> GenerateMoratoriumAnnuityPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new Exception("Effective Date should be less than the maturity date");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPrincipalPayments = CalculateNumberOfInstallments(loanInput.principalFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value;

            var principalPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.principalFrequency).Value;

            int principalPaymentMultiple = Convert.ToInt32(12 / principalPaymentsInAYear);

            var principalFirstPaymentNumber = (numberOfPayments - numberOfPrincipalPayments) + 1;

            Double FV = 0;

            FinancialTypes.UNEQUALLOANPAYMENTS_table result;


            //result = wct.AMORTSCHED(loanInput.principalAmount, loanInput.effectiveDate, (loanInput.interestRate / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            result = wct.UNEQUALLOANPAYMENTS(loanInput.principalAmount, (loanInput.interestRate / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, principalPaymentMultiple, principalFirstPaymentNumber, numberOfPayments, wct.NULL_INT, FV, true);


            int counter = 0;
            foreach (DataRow row in result.Rows)
            {
                LoanPaymentSchedulePeriodicViewModel payment = new LoanPaymentSchedulePeriodicViewModel();
                payment.paymentNumber = Convert.ToInt32(row["num_pmt"]);
                payment.paymentDate = Convert.ToDateTime(row["date_pmt"]);
                payment.startPrincipalAmount = Convert.ToDouble(row["amt_prin_init"]);
                payment.periodPaymentAmount = Convert.ToDouble(row["amt_pmt"]);
                payment.periodInterestAmount = Convert.ToDouble(row["amt_int_pay"]);
                payment.periodPrincipalAmount = Convert.ToDouble(row["amt_prin_pay"]);
                payment.endPrincipalAmount = Convert.ToDouble(row["amt_prin_end"]);
                payment.interestRate = loanInput.interestRate / 100.0;

                output.Add(payment);

                counter += 1;
            }

            var maturityDate = output.Max(x => x.paymentDate); //loanInput.maturityDate

            var internalRateOfReturn = CalculateIRR(loanInput, output, numberOfPayments, numberOfPaymentsInAYear,
                                             daysInAYear, FV, "U", maturityDate);

            FinancialTypes.UNEQUALLOANPAYMENTS_table amortisedResult;

            amortisedResult = wct.UNEQUALLOANPAYMENTS(loanInput.principalAmount - loanInput.integralFeeAmount, (internalRateOfReturn / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, principalPaymentMultiple, principalFirstPaymentNumber, numberOfPayments, wct.NULL_INT, FV, true);

            counter = 0;
            foreach (var payment in output)
            {
                payment.amortisedStartPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_init"]);
                payment.amortisedPeriodPaymentAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_pmt"]);
                payment.amortisedPeriodInterestAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_int_pay"]);
                payment.amortisedPeriodPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_pay"]);
                payment.amortisedEndPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_end"]);
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return output;

        }

        private List<LoanPaymentSchedulePeriodicViewModel> GenerateBallonPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new Exception("Effective Date should be less than the maturity date");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value;            

            int principalPaymentMultiple = 1;

            var principalFirstPaymentNumber = numberOfPayments;

            Double FV = 0;

            FinancialTypes.UNEQUALLOANPAYMENTS_table result;


            result = wct.UNEQUALLOANPAYMENTS(loanInput.principalAmount, (loanInput.interestRate / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, principalPaymentMultiple, principalFirstPaymentNumber, numberOfPayments, wct.NULL_INT, FV, true);


            int counter = 0;
            foreach (DataRow row in result.Rows)
            {
                LoanPaymentSchedulePeriodicViewModel payment = new LoanPaymentSchedulePeriodicViewModel();
                payment.paymentNumber = Convert.ToInt32(row["num_pmt"]);
                payment.paymentDate = Convert.ToDateTime(row["date_pmt"]);
                payment.startPrincipalAmount = Convert.ToDouble(row["amt_prin_init"]);
                payment.periodPaymentAmount = Convert.ToDouble(row["amt_pmt"]);
                payment.periodInterestAmount = Convert.ToDouble(row["amt_int_pay"]);
                payment.periodPrincipalAmount = Convert.ToDouble(row["amt_prin_pay"]);
                payment.endPrincipalAmount = Convert.ToDouble(row["amt_prin_end"]);
                payment.interestRate = loanInput.interestRate / 100.0;

                output.Add(payment);

                counter += 1;
            }

            var maturityDate = output.Max(x => x.paymentDate); //loanInput.maturityDate

            var internalRateOfReturn = CalculateIRR(loanInput, output, numberOfPayments, numberOfPaymentsInAYear,
                                             daysInAYear, FV, "U", maturityDate);

            FinancialTypes.UNEQUALLOANPAYMENTS_table amortisedResult;

            amortisedResult = wct.UNEQUALLOANPAYMENTS(loanInput.principalAmount - loanInput.integralFeeAmount, (internalRateOfReturn / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, principalPaymentMultiple, principalFirstPaymentNumber, numberOfPayments, wct.NULL_INT, FV, true);

            counter = 0;
            foreach (var payment in output)
            {
                payment.amortisedStartPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_init"]);
                payment.amortisedPeriodPaymentAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_pmt"]);
                payment.amortisedPeriodInterestAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_int_pay"]);
                payment.amortisedPeriodPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_pay"]);
                payment.amortisedEndPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_end"]);
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return output;

        }
        /// <summary>
        /// USE FOR REDUCING BALANCE
        /// </summary>
        /// <param name="loanInput"></param>
        /// <returns></returns>         
        private List<LoanPaymentSchedulePeriodicViewModel> GenerateReducingBalancePeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPrincipalPayments = CalculateNumberOfInstallments(loanInput.principalFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value;

            var principalPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.principalFrequency).Value;

            int principalPaymentMultiple = Convert.ToInt32(12 / principalPaymentsInAYear);

            var principalFirstPaymentNumber = (numberOfPayments - numberOfPrincipalPayments) + 1;            

            int lastPaymentNumber = wct.NULL_INT;
            Double FV = wct.NULL_DOUBLE;
            Double PPMT = wct.NULL_DOUBLE;
            Boolean eom = true;

            FinancialTypes.CONSTPRINAMORT_table result;

            //result = wct.UNEQUALLOANPAYMENTS(loanInput.principalAmount, (loanInput.interestRate / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, principalPaymentMultiple, principalFirstPaymentNumber, numberOfPayments, wct.NULL_INT, FV, true);

            result = wct.CONSTPRINAMORT(loanInput.principalAmount, (loanInput.interestRate / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, numberOfPayments, lastPaymentNumber, principalFirstPaymentNumber, FV, PPMT, eom);

            int counter = 0;
            foreach (DataRow row in result.Rows)
            {
                LoanPaymentSchedulePeriodicViewModel payment = new LoanPaymentSchedulePeriodicViewModel();
                payment.paymentNumber = Convert.ToInt32(row["num_pmt"]);
                payment.paymentDate = Convert.ToDateTime(row["date_pmt"]);
                payment.startPrincipalAmount = Convert.ToDouble(row["amt_prin_init"]);
                payment.periodPaymentAmount = Convert.ToDouble(row["amt_pmt"]);
                payment.periodInterestAmount = Convert.ToDouble(row["amt_int_pay"]);
                payment.periodPrincipalAmount = Convert.ToDouble(row["amt_prin_pay"]);
                payment.endPrincipalAmount = Convert.ToDouble(row["amt_prin_end"]);
                payment.interestRate = loanInput.interestRate / 100.0;

                output.Add(payment);

                counter += 1;
            }

            var maturityDate = output.Max(x => x.paymentDate); //loanInput.maturityDate

            var internalRateOfReturn = CalculateIRR(loanInput, output, numberOfPayments, numberOfPaymentsInAYear,
                                             daysInAYear, FV, "U", maturityDate);

            FinancialTypes.CONSTPRINAMORT_table amortisedResult;

            //amortisedResult = wct.UNEQUALLOANPAYMENTS(loanInput.principalAmount - loanInput.integralFeeAmount, (internalRateOfReturn / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, principalPaymentMultiple, principalFirstPaymentNumber, numberOfPayments, wct.NULL_INT, FV, true);

            amortisedResult = wct.CONSTPRINAMORT(loanInput.principalAmount - loanInput.integralFeeAmount, (internalRateOfReturn / 100.0), loanInput.effectiveDate, numberOfPaymentsInAYear, loanInput.interestFirstpaymentDate, daysInAYear, numberOfPayments, lastPaymentNumber, principalFirstPaymentNumber, FV, PPMT, eom);

            counter = 0;
            foreach (var payment in output)
            {
                payment.amortisedStartPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_init"]);
                payment.amortisedPeriodPaymentAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_pmt"]);
                payment.amortisedPeriodInterestAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_int_pay"]);
                payment.amortisedPeriodPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_pay"]);
                payment.amortisedEndPrincipalAmount = Convert.ToDouble(amortisedResult.Rows[counter]["amt_prin_end"]);
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return output;

        }

        public List<LoanPaymentScheduleDailyViewModel> GenerateDailyLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            List<LoanPaymentSchedulePeriodicViewModel> periodicSchedule = GeneratePeriodicLoanSchedule(loanInput);

            List<LoanPaymentScheduleDailyViewModel> output = new List<LoanPaymentScheduleDailyViewModel>();

            int counter = 1;
            foreach (var item in periodicSchedule)
            {

                //output.Add(payment);

                counter += 1;
            }

            return output;

        }

        private IEnumerable<LoanPaymentScheduleDailyViewModel> GenerateDailyScheduleRange(LoanPaymentSchedulePeriodicViewModel periodicSchedule, DateTime nextPaymentDate, int lastRowCount)
        {
            List<LoanPaymentScheduleDailyViewModel> output = new List<LoanPaymentScheduleDailyViewModel>();

            var dateDifferenceCount = (periodicSchedule.paymentDate - nextPaymentDate).TotalDays;

            for (int counter = 1; counter <= dateDifferenceCount; counter++)
            {

            }

            return output;

        }


        private List<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularPeriodicScheduleWithAmortisedCost(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.irregularPaymentSchedule.Count() == 0)
                throw new Exception("Specify a repayment schedule");

            if (loanInput.principalAmount != (loanInput.irregularPaymentSchedule.Sum(x => x.paymentAmount)))
                throw new Exception("Payment Amount is not equal to the principal Amount");

            if (loanInput.effectiveDate > (loanInput.irregularPaymentSchedule.Min(x => x.paymentDate)))
                throw new Exception("Effective Date should be less than the payment date(s)");

            List<LoanPaymentSchedulePeriodicViewModel> paymentSchedule = GenerateIrregularPeriodicSchedule(loanInput);

            var amounts = paymentSchedule.Select(x => x.periodPaymentAmount).ToList();  //paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
            var dates = paymentSchedule.Select(x => x.paymentDate).ToList();

            amounts[0] = amounts[0] * -1;
            var internalRateOfReturn = wct.XIRR(amounts, dates, wct.NULL_DOUBLE);

            //FinancialTypes.AMORTSCHED_table amortisedResult;
            //amortisedResult = wct.AMORTSCHED(loanInput.principalAmount - loanInput.integralFeeAmount, loanInput.effectiveDate, (internalRateOfReturn / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            loanInput.principalAmount = loanInput.principalAmount - loanInput.integralFeeAmount;
            loanInput.interestRate = internalRateOfReturn / 100.0;
            List<LoanPaymentSchedulePeriodicViewModel> paymentScheduleArmotised = GenerateIrregularPeriodicSchedule(loanInput);

            int counter = 0;
            foreach (var payment in paymentSchedule)
            {
                payment.amortisedStartPrincipalAmount = paymentScheduleArmotised[counter].startPrincipalAmount;
                payment.amortisedPeriodPaymentAmount = paymentScheduleArmotised[counter].periodPaymentAmount;
                payment.amortisedPeriodInterestAmount = paymentScheduleArmotised[counter].periodInterestAmount;
                payment.amortisedPeriodPrincipalAmount = paymentScheduleArmotised[counter].periodPrincipalAmount;
                payment.amortisedEndPrincipalAmount = paymentScheduleArmotised[counter].endPrincipalAmount;
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return paymentSchedule;
        }


        private List<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            output.Add(new LoanPaymentSchedulePeriodicViewModel
            {
                paymentNumber = 0,
                paymentDate = loanInput.effectiveDate,
                startPrincipalAmount = 0,
                periodPrincipalAmount = 0,
                amortisedPeriodInterestAmount = 0,
                periodPaymentAmount = 0,
                endPrincipalAmount = loanInput.principalAmount
            });


            var data = loanInput.irregularPaymentSchedule.OrderBy(x => x.paymentDate);

            int paymentNumber = 1;
            double previousPrincipalAmount = loanInput.principalAmount;
            DateTime previousPaymentDate = loanInput.effectiveDate;
            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);


            foreach (var item in data)
            {
                LoanPaymentSchedulePeriodicViewModel loanPeriod = new LoanPaymentSchedulePeriodicViewModel();
                loanPeriod.paymentNumber = paymentNumber;
                loanPeriod.paymentDate = item.paymentDate;
                loanPeriod.startPrincipalAmount = previousPrincipalAmount;
                loanPeriod.periodPrincipalAmount = item.paymentAmount;

                var dateDifferenceCount = (item.paymentDate - previousPaymentDate).TotalDays;

                loanPeriod.periodInterestAmount = (previousPrincipalAmount * (loanInput.interestRate / 100.0)) * (dateDifferenceCount / daysInAYear);
                loanPeriod.periodPaymentAmount = loanPeriod.periodPrincipalAmount + loanPeriod.periodInterestAmount;
                loanPeriod.endPrincipalAmount = loanPeriod.startPrincipalAmount - loanPeriod.periodPrincipalAmount;

                output.Add(loanPeriod);

                previousPrincipalAmount = loanPeriod.endPrincipalAmount;
                previousPaymentDate = loanPeriod.paymentDate;
                paymentNumber += 1;

            }

            return output;

        }

        private List<LoanPaymentSchedulePeriodicViewModel> GenerateBulletPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            output.Add(new LoanPaymentSchedulePeriodicViewModel
            {
                paymentNumber = 0,
                paymentDate = loanInput.effectiveDate,
                startPrincipalAmount = 0,
                periodPrincipalAmount = 0,
                amortisedPeriodInterestAmount = 0,
                periodPaymentAmount = 0,
                endPrincipalAmount = loanInput.principalAmount
            });


            //double previousPrincipalAmount = loanInput.principalAmount;
            //DateTime previousPaymentDate = loanInput.effectiveDate;
            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);


            LoanPaymentSchedulePeriodicViewModel loanPeriod = new LoanPaymentSchedulePeriodicViewModel();
            loanPeriod.paymentNumber = 1;
            loanPeriod.paymentDate = loanInput.maturityDate;
            loanPeriod.startPrincipalAmount = loanInput.principalAmount;
            loanPeriod.periodPrincipalAmount = loanInput.principalAmount;

            var dateDifferenceCount = (loanInput.maturityDate - loanInput.effectiveDate).TotalDays;

            loanPeriod.periodInterestAmount = (loanInput.principalAmount * (loanInput.interestRate / 100.0)) * (dateDifferenceCount / daysInAYear);
            loanPeriod.periodPaymentAmount = loanPeriod.periodPrincipalAmount + loanPeriod.periodInterestAmount;
            loanPeriod.endPrincipalAmount = loanPeriod.startPrincipalAmount - loanPeriod.periodPrincipalAmount;

            output.Add(loanPeriod);

            return output;
        }

        private List<LoanPaymentSchedulePeriodicViewModel> GenerateBulletPeriodicScheduleWithAmortisedCost(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new Exception("Effective Date should be less than the maturity date");

            List<LoanPaymentSchedulePeriodicViewModel> paymentSchedule = GenerateBulletPeriodicSchedule(loanInput);

            var amounts = paymentSchedule.Select(x => x.periodPaymentAmount).ToList();  //paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
            var dates = paymentSchedule.Select(x => x.paymentDate).ToList();

            amounts[0] = amounts[0] * -1;
            var internalRateOfReturn = wct.XIRR(amounts, dates, wct.NULL_DOUBLE);


            loanInput.principalAmount = loanInput.principalAmount - loanInput.integralFeeAmount;
            loanInput.interestRate = internalRateOfReturn / 100.0;
            List<LoanPaymentSchedulePeriodicViewModel> paymentScheduleArmotised = GenerateBulletPeriodicSchedule(loanInput);

            int counter = 0;
            foreach (var payment in paymentSchedule)
            {
                payment.amortisedStartPrincipalAmount = paymentScheduleArmotised[counter].startPrincipalAmount;
                payment.amortisedPeriodPaymentAmount = paymentScheduleArmotised[counter].periodPaymentAmount;
                payment.amortisedPeriodInterestAmount = paymentScheduleArmotised[counter].periodInterestAmount;
                payment.amortisedPeriodPrincipalAmount = paymentScheduleArmotised[counter].periodPrincipalAmount;
                payment.amortisedEndPrincipalAmount = paymentScheduleArmotised[counter].endPrincipalAmount;
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return paymentSchedule;
        }

        private List<LoanPaymentSchedulePeriodicViewModel> GenerateConstantPrincipalAndInterestPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            
            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);

            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPrincipalPayments = CalculateNumberOfInstallments(loanInput.principalFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.interestFrequency).Value;

            var principalPaymentsInAYear = (int)context.tbl_Frequency_Type.FirstOrDefault(x => x.FrequencyTypeId == loanInput.principalFrequency).Value;

            int principalPaymentMultiple = Convert.ToInt32(12 / principalPaymentsInAYear);

            var principalFirstPaymentNumber = (numberOfPayments - numberOfPrincipalPayments) + 1;

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            output.Add(new LoanPaymentSchedulePeriodicViewModel
            {
                paymentNumber = 0,
                paymentDate = loanInput.effectiveDate,
                startPrincipalAmount = 0,
                periodPrincipalAmount = 0,
                amortisedPeriodInterestAmount = 0,
                periodPaymentAmount = 0,
                endPrincipalAmount = loanInput.principalAmount
            });


            double previousPrincipalAmount = loanInput.principalAmount;
            DateTime nextPaymentDate = loanInput.interestFirstpaymentDate;
            double periodPrincipalAmount = loanInput.principalAmount / numberOfPayments;
            double interestRate = loanInput.interestRate / 100.0;
            double periodInterestAmount = loanInput.principalAmount * (interestRate / numberOfPaymentsInAYear) * numberOfPayments;

            for (int i = 1; i <= numberOfPayments; i++)
            {
                LoanPaymentSchedulePeriodicViewModel loanPeriod = new LoanPaymentSchedulePeriodicViewModel();
                loanPeriod.paymentNumber = i;
                loanPeriod.paymentDate = nextPaymentDate;
                loanPeriod.startPrincipalAmount = previousPrincipalAmount;
                loanPeriod.periodPrincipalAmount = periodPrincipalAmount;

                loanPeriod.periodInterestAmount = periodInterestAmount;
                loanPeriod.periodPaymentAmount = loanPeriod.periodPrincipalAmount + loanPeriod.periodInterestAmount;
                loanPeriod.endPrincipalAmount = loanPeriod.startPrincipalAmount - loanPeriod.periodPrincipalAmount;

                output.Add(loanPeriod);

                previousPrincipalAmount = loanPeriod.endPrincipalAmount;
                nextPaymentDate = wct.NPD(loanPeriod.paymentDate, loanInput.interestFirstpaymentDate, numberOfPaymentsInAYear, numberOfPayments);
            }

            return output;

        }

        private List<LoanPaymentSchedulePeriodicViewModel> GenerateConstantPrincipalAndInterestPeriodicScheduleWithAmortisedCost(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new Exception("Effective Date should be less than the maturity date");

            List<LoanPaymentSchedulePeriodicViewModel> paymentSchedule = GenerateConstantPrincipalAndInterestPeriodicSchedule(loanInput);

            var amounts = paymentSchedule.Select(x => x.periodPaymentAmount).ToList();  //paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
            var dates = paymentSchedule.Select(x => x.paymentDate).ToList();

            amounts[0] = amounts[0] * -1;
            var internalRateOfReturn = wct.XIRR(amounts, dates, wct.NULL_DOUBLE);


            loanInput.principalAmount = loanInput.principalAmount - loanInput.integralFeeAmount;
            loanInput.interestRate = internalRateOfReturn / 100.0;
            List<LoanPaymentSchedulePeriodicViewModel> paymentScheduleArmotised = GenerateConstantPrincipalAndInterestPeriodicSchedule(loanInput);

            int counter = 0;
            foreach (var payment in paymentSchedule)
            {
                payment.amortisedStartPrincipalAmount = paymentScheduleArmotised[counter].startPrincipalAmount;
                payment.amortisedPeriodPaymentAmount = paymentScheduleArmotised[counter].periodPaymentAmount;
                payment.amortisedPeriodInterestAmount = paymentScheduleArmotised[counter].periodInterestAmount;
                payment.amortisedPeriodPrincipalAmount = paymentScheduleArmotised[counter].periodPrincipalAmount;
                payment.amortisedEndPrincipalAmount = paymentScheduleArmotised[counter].endPrincipalAmount;
                payment.internalRateOfReturn = internalRateOfReturn / 100.0;

                counter += 1;
            }

            return paymentSchedule;
        }
    }
}
