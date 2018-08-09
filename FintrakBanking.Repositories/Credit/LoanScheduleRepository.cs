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
using System.ServiceModel;
using FintrakBanking.Interfaces.Setups.General;
using XLeratorDLL_financial;
using OfficeOpenXml.Style;
using System.Drawing;
using OfficeOpenXml;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.Repositories.Credit
{
    using wct = XLeratorDLL_financial.XLeratorDLL_financial;
    using FinancialTypes = XLeratorDLL_financial.FinancialTypes;

     
    public class LoanScheduleRepository: ILoanScheduleRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;

        public LoanScheduleRepository(FinTrakBankingContext _context, IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
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

            var frequencyValue = context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == frequencyTypeId).VALUE;

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
                numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 2.0;
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
            var frequencyValue = context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == frequencyTypeId).VALUE;

            DateTime output = effectiveDate.AddMonths(12 / (int)frequencyValue);
            return output;
        }

        public IEnumerable<LookupViewModel> GetAllLoanScheduleCategory()
        {
            return (from data in context.TBL_LOAN_SCHEDULE_CATEGORY
                    select new LookupViewModel()
                    {
                        lookupId = data.SCHEDULECATEGORYID,
                        lookupName = data.SCHEDULECATEGORYNAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllLoanScheduleType()
        {
            return (from data in context.TBL_LOAN_SCHEDULE_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.SCHEDULETYPEID,
                        lookupName = data.SCHEDULETYPENAME,
                        lookupTypeId = data.SCHEDULECATEGORYID,
                        lookupTypeName = data.TBL_LOAN_SCHEDULE_CATEGORY.SCHEDULECATEGORYNAME
                    });
        }


        public IEnumerable<LookupViewModel> GetAllLoanScheduleType(short? productTypeId)
        {
            if (productTypeId == null || productTypeId <= 0)
            {
                return (from data in context.TBL_LOAN_SCHEDULE_TYPE
                        select new LookupViewModel()
                        {
                            lookupId = data.SCHEDULETYPEID,
                            lookupName = data.SCHEDULETYPENAME,
                            lookupTypeId = data.SCHEDULECATEGORYID,
                            lookupTypeName = data.TBL_LOAN_SCHEDULE_CATEGORY.SCHEDULECATEGORYNAME
                        });

            }
            else
            {
                return (from data in context.TBL_LOAN_SCHEDULE_TYPE
                        join t in context.TBL_LOAN_SCHEDULE_TYPE_PRODUCT
                        on data.SCHEDULETYPEID equals t.SCHEDULETYPEID
                        where t.PRODUCTTYPEID == productTypeId
                        select new LookupViewModel()
                        {
                            lookupId = data.SCHEDULETYPEID,
                            lookupName = data.SCHEDULETYPENAME,
                            lookupTypeId = data.SCHEDULECATEGORYID,
                            lookupTypeName = data.TBL_LOAN_SCHEDULE_CATEGORY.SCHEDULECATEGORYNAME
                        });
            }
        }



        public IEnumerable<LookupViewModel> GetLoanScheduleTypeByCategory(short categoryId)
        {
            return (from data in context.TBL_LOAN_SCHEDULE_TYPE
                    where data.SCHEDULECATEGORYID == categoryId
                    select new LookupViewModel()
                    {
                        lookupId = data.SCHEDULETYPEID,
                        lookupName = data.SCHEDULETYPENAME,
                        lookupTypeId = data.SCHEDULECATEGORYID,
                        lookupTypeName = data.TBL_LOAN_SCHEDULE_CATEGORY.SCHEDULECATEGORYNAME
                    });
        }

        public List<LoanPaymentSchedulePeriodicViewModel> GeneratePeriodicLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.principalAmount <= 0)
                throw new ConditionNotMetException("Please Enter Loan Amount");
            if (loanInput.interestRate < 0)
                throw new ConditionNotMetException("Please Enter Loan Interest Amount");
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

        public byte[] GenerateLoanScheduleExport(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.principalAmount <= 0)
                throw new ConditionNotMetException("Please Enter Loan Amount");
            if (loanInput.interestRate < 0)
                throw new ConditionNotMetException("Please Enter Loan Interest Amount");
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

            Byte[] fileBytes = null;

            if (output != null)
            {
               
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("SearchReport");
                    ws.DefaultColWidth = 20;
                    ws.Cells.Style.WrapText = true;

                    ws.Cells["A2:F2"].Merge = true;
                    ws.Cells["A3:F3"].Merge = true;
                    ws.Cells["A4:F4"].Merge = true;
                    ws.Cells["A5:F5"].Merge = true;

                    ws.Cells["A2:F2"].Style.Font.Bold = true;
                    ws.Cells["A3:F3"].Style.Font.Bold = true;
                    ws.Cells["A4:F4"].Style.Font.Bold = true;
                    ws.Cells["A5:F5"].Style.Font.Bold = true;

                    ws.Cells[2, 1].Value = "GRANTED AMOUNT  :  " + loanInput.principalAmount;
                    ws.Cells[3, 1].Value = "INTEREST AMOUNT  :  " + loanInput.interestRate;
                    ws.Cells[4, 1].Value = "EFFECTIVE DATE  :  " + loanInput.effectiveDate;
                    ws.Cells[5, 1].Value = "MATURITY DATE  :  " + loanInput.maturityDate;

                    ws.Cells[7, 1].Value = "Payment Number";
                    ws.Cells[7, 2].Value = "Payment Date";
                    ws.Cells[7, 3].Value = "Start Principal";
                    ws.Cells[7, 4].Value = "Period Amount";
                    ws.Cells[7, 5].Value = "Principal Amount";
                    ws.Cells[7, 6].Value = "Interest Amount";
                    ws.Cells[7, 7].Value = "Balance";
                    ws.Cells[7, 8].Value = "AM Start Principal";
                    ws.Cells[7, 9].Value = "AM Periodic Amount";
                    ws.Cells[7, 10].Value = "AM Principal Amount";
                    ws.Cells[7, 11].Value = "AM Interest Amount";
                    ws.Cells[7, 12].Value = "AM Balancet";


                    Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#B8860B");
                    ws.Cells["A7:K8"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells["A7:K8"].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    ws.Cells["A7:K8"].Style.Font.Bold = true;

                    ws.Cells["A7:K8"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells["A7:K8"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells["A7:K8"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells["A7:K8"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    for (int i = 8; i <= output.Count + 7; i++)
                    {
                        var record = output[i - 8];
                        ws.Cells[i, 1].Value = i - 7;
                        ws.Cells[i, 2].Value = record.paymentDate;
                        ws.Cells[i, 3].Value = record.startPrincipalAmount;
                        ws.Cells[i, 4].Value = record.periodPaymentAmount;
                        ws.Cells[i, 5].Value = record.periodPrincipalAmount;
                        ws.Cells[i, 6].Value = record.periodInterestAmount;
                        ws.Cells[i, 7].Value = record.endPrincipalAmount;
                        ws.Cells[i, 8].Value = record.amortisedStartPrincipalAmount;
                        ws.Cells[i, 9].Value = record.amortisedPeriodPaymentAmount;
                        ws.Cells[i, 10].Value = record.amortisedPeriodPrincipalAmount;
                        ws.Cells[i, 11].Value = record.amortisedPeriodInterestAmount;
                        ws.Cells[i, 12].Value = record.amortisedEndPrincipalAmount;

                    }
                    fileBytes = pck.GetAsByteArray();
                }


            }

            return fileBytes;
        }
        private int GetDaysInAYear(DayCountConventionEnum dayCountId)
        {
            if (dayCountId == DayCountConventionEnum.Actual_Actual)
            {
                var currentDate = DateTime.Now;
                var firstDate = new DateTime(currentDate.Year, 1, 1); //  DateTime.ParseExact(user, "MM-dd-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                var lastdate = new DateTime(currentDate.Year, 12, 31);
                var difference = (lastdate - firstDate).TotalDays + 1;

                return Convert.ToInt32(difference);
            }

            var value = context.TBL_DAY_COUNT_CONVENTION.FirstOrDefault(x => x.DAYCOUNTCONVENTIONID == (short)dayCountId).DAYSINAYEAR;

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
                /// modify by Gbenga to be approve by Anu(Reasons that var maturityDate = output.Max(x => x.paymentDate) makes MaturityDate and FirstPaymentDate qual)
                if (period.Days == 0)
                {
                    output = output * (daysInAYear / 1);
                }
                else
                {
                    output = output * (daysInAYear / period.Days);
                }

                //output = output * (daysInAYear / period.Days);
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
            if (loanInput.effectiveDate == null)
                throw new ConditionNotMetException("Please Enter Effective Date");
            if (loanInput.maturityDate == null)
                throw new ConditionNotMetException("Please Enter Maturity Date");
            if (loanInput.interestFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Interest Payment Date");
            if (loanInput.principalFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Principal Payment Date");
            if (loanInput.principalAmount <= 0)
                throw new ConditionNotMetException("Please Enter Loan Amount");
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("Effective Date must be less than the maturity date");
            if (loanInput.interestFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Interest Payment Date must be less than the maturity date");
            if (loanInput.principalFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Principal Payment Date must be less than the maturity date");
            if (loanInput.effectiveDate > loanInput.principalFirstpaymentDate)
                throw new ConditionNotMetException("First Principal Payment Date cannot be less than the effective date");
            if (loanInput.effectiveDate > loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("First Interest Payment Date cannot be less than the effective date");
            if (loanInput.principalFirstpaymentDate < loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("Principal first payment date cannot be less than the interest first payment date");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accrualBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);
            int numberOfPaymentsInAYear = (int)context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == loanInput.interestFrequency).VALUE;

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
                payment.interestRate = loanInput.interestRate;

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
                payment.effectiveInterestRate = internalRateOfReturn;

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
            int principalPaymentMultiple;
            int ppm;
            if (loanInput.effectiveDate == null)
                throw new ConditionNotMetException("Please Enter Effective Date");
            if (loanInput.maturityDate == null)
                throw new ConditionNotMetException("Please Enter Maturity Date");
            if (loanInput.interestFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Interest Payment Date");
            if (loanInput.principalFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Principal Payment Date");
            if (loanInput.principalAmount <= 0)
                throw new ConditionNotMetException("Please Enter Loan Amount");
            if (loanInput.principalFrequency <= 0)
                throw new ConditionNotMetException("Please Select Principal Frequency");
            if (loanInput.interestFrequency <= 0)
                throw new ConditionNotMetException("Please Select Interest Frequency");
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("Effective Date must be less than the maturity date");
            if (loanInput.interestFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Interest Payment Date must be less than the maturity date");
            if (loanInput.principalFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Principal Payment Date must be less than the maturity date");
            if (loanInput.effectiveDate > loanInput.principalFirstpaymentDate)
                throw new ConditionNotMetException("First Principal Payment Date cannot be less than the effective date");
            if (loanInput.effectiveDate > loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("First Interest Payment Date cannot be less than the effective date");
            if (loanInput.principalFirstpaymentDate < loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("Principal first payment date cannot be less than the interest first payment date");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accrualBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPrincipalPayments = CalculateNumberOfInstallments(loanInput.principalFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == loanInput.interestFrequency).VALUE;

            var principalPaymentsInAYear = (int)context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == loanInput.principalFrequency).VALUE;

            //principalPaymentMultiple  = Convert.ToInt32(12 / principalPaymentsInAYear);

            ppm = Convert.ToInt32(12 / principalPaymentsInAYear);

            if (ppm == 0)
            {
                principalPaymentMultiple = 1;
            }
            else
            {
                principalPaymentMultiple = ppm;
            }

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
                payment.interestRate = loanInput.interestRate;

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
                payment.effectiveInterestRate = internalRateOfReturn;

                counter += 1;
            }

            return output;

        }

        private List<LoanPaymentSchedulePeriodicViewModel> GenerateBallonPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            if(loanInput.effectiveDate == null )
                throw new ConditionNotMetException("Please Enter Effective Date");
            if (loanInput.maturityDate == null)
                throw new ConditionNotMetException("Please Enter Maturity Date");
            if (loanInput.principalFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Principal Payment Date");
            if (loanInput.principalAmount <= 0)
                throw new ConditionNotMetException("Please Enter Loan Amount");
            if (loanInput.interestFrequency <= 0)
                throw new ConditionNotMetException("Please Select Interest Frequency");
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("Effective Date must be less than the maturity date");
            if (loanInput.principalFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Principal Payment Date must be less than the maturity date");
            if (loanInput.effectiveDate > loanInput.principalFirstpaymentDate)
                throw new ConditionNotMetException("First Principal Payment Date cannot be less than the effective date");
            if (loanInput.principalFirstpaymentDate < loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("Principal first payment date cannot be less than the interest first payment date");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accrualBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == loanInput.interestFrequency).VALUE;            

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
                payment.interestRate = loanInput.interestRate;

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
                payment.effectiveInterestRate = internalRateOfReturn;

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
            if (loanInput.effectiveDate == null)
                throw new ConditionNotMetException("Please Enter Effective Date");
            if (loanInput.maturityDate == null)
                throw new ConditionNotMetException("Please Enter Maturity Date");
            if (loanInput.interestFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Interest Payment Date");
            if (loanInput.principalFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Principal Payment Date");
            if (loanInput.principalAmount <= 0)
                throw new ConditionNotMetException("Please Enter Loan Amount");
            if (loanInput.principalFrequency <= 0)
                throw new ConditionNotMetException("Please Select Principal Frequency");
            if (loanInput.interestFrequency <= 0)
                throw new ConditionNotMetException("Please Select Interest Frequency");
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("Effective Date must be less than the maturity date");
            if (loanInput.interestFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Interest Payment Date must be less than the maturity date");
            if (loanInput.principalFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Principal Payment Date must be less than the maturity date");
            if (loanInput.effectiveDate > loanInput.principalFirstpaymentDate)
                throw new ConditionNotMetException("First Principal Payment Date cannot be less than the effective date");
            if (loanInput.effectiveDate > loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("First Interest Payment Date cannot be less than the effective date");
            if (loanInput.principalFirstpaymentDate < loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("Principal first payment date cannot be less than the interest first payment date");

            List<LoanPaymentSchedulePeriodicViewModel> output = new List<LoanPaymentSchedulePeriodicViewModel>();

            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accrualBasis);
            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPrincipalPayments = CalculateNumberOfInstallments(loanInput.principalFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == loanInput.interestFrequency).VALUE;

            var principalPaymentsInAYear = (int)context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == loanInput.principalFrequency).VALUE;

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
                payment.interestRate = loanInput.interestRate;

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
                payment.effectiveInterestRate = internalRateOfReturn;

                counter += 1;
            }

            return output;

        }

        public List<LoanPaymentScheduleDailyViewModel> GenerateDailyLoanSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            List<LoanPaymentSchedulePeriodicViewModel> periodicSchedule = GeneratePeriodicLoanSchedule(loanInput);

            List<LoanPaymentScheduleDailyViewModel> output = new List<LoanPaymentScheduleDailyViewModel>();

            var numberOfPeriods = periodicSchedule.Count() - 1;

            DateTime previousPaymentDate = loanInput.effectiveDate;

            int dailyScheduleRowCount = 0;
            int paymentNumber = 1;
            foreach (var item in periodicSchedule)
            {

                if (paymentNumber > 1)
                {
                    //output.AddRange(GenerateDailyScheduleRange(item, previousPaymentDate));

                    var dateDifferenceCount = (item.paymentDate - previousPaymentDate).TotalDays;
                    var currentDate = previousPaymentDate; // item.paymentDate;
                    double previousPrincipalAmount = item.startPrincipalAmount;
                    double amPreviousPrincipalAmount = item.amortisedStartPrincipalAmount;
                    double accuredInterest = 0;
                    double amAccuredInterest = 0;

                    for (int counter = 1; counter <= dateDifferenceCount; counter++)
                    {
                        LoanPaymentScheduleDailyViewModel dayValues = new LoanPaymentScheduleDailyViewModel();
                        dayValues.paymentNumber = dailyScheduleRowCount;
                        dayValues.paymentDate = item.paymentDate;
                        dayValues.date = currentDate;

                        dayValues.openingBalance = previousPrincipalAmount;
                        dayValues.startPrincipalAmount = item.startPrincipalAmount;
                        dayValues.dailyPrincipalAmount = item.periodPrincipalAmount / dateDifferenceCount;
                        dayValues.dailyInterestAmount = item.periodInterestAmount / dateDifferenceCount;
                        dayValues.dailyPaymentAmount = dayValues.dailyPrincipalAmount + dayValues.dailyInterestAmount;
                        dayValues.closingBalance = dayValues.openingBalance - dayValues.dailyPrincipalAmount;
                        dayValues.endPrincipalAmount = item.endPrincipalAmount;
                        dayValues.accruedInterest = accuredInterest + dayValues.dailyInterestAmount;
                        dayValues.amortisedCost = dayValues.startPrincipalAmount + dayValues.accruedInterest;
                        dayValues.norminalInterestRate = loanInput.interestRate;

                        dayValues.amOpeningBalance = amPreviousPrincipalAmount;
                        dayValues.amStartPrincipalAmount = item.amortisedStartPrincipalAmount;
                        dayValues.amDailyPrincipalAmount = item.amortisedPeriodPrincipalAmount / dateDifferenceCount;
                        dayValues.amDailyInterestAmount = item.amortisedPeriodInterestAmount / dateDifferenceCount;
                        dayValues.amDailyPaymentAmount = dayValues.amDailyPrincipalAmount + dayValues.amDailyInterestAmount;
                        dayValues.amClosingBalance = dayValues.amOpeningBalance - dayValues.amDailyPrincipalAmount;
                        dayValues.amEndPrincipalAmount = item.amortisedEndPrincipalAmount;
                        dayValues.amAccruedInterest = amAccuredInterest + dayValues.amDailyInterestAmount;
                        dayValues.amAmortisedCost = dayValues.amStartPrincipalAmount + dayValues.amAccruedInterest;

                        dayValues.discountPremium = dayValues.amDailyInterestAmount - dayValues.dailyInterestAmount;
                        dayValues.unEarnedFee = dayValues.amClosingBalance - dayValues.closingBalance;
                        dayValues.earnedFee = loanInput.integralFeeAmount - dayValues.unEarnedFee;
                        dayValues.effectiveInterestRate = item.effectiveInterestRate;
                        dayValues.numberOfPeriods = numberOfPeriods; 

                        //public double balloonAmt { get; set; }


                        output.Add(dayValues);

                        accuredInterest = dayValues.accruedInterest;
                        amAccuredInterest = dayValues.amAccruedInterest;

                        previousPrincipalAmount = dayValues.closingBalance;
                        amPreviousPrincipalAmount = dayValues.amClosingBalance;

                        currentDate = currentDate.AddDays(1);
                        dailyScheduleRowCount += 1;

                    }

                    previousPaymentDate = item.paymentDate;                    
                }

                paymentNumber += 1;
            }

            return output;

        }

        //private IEnumerable<LoanPaymentScheduleDailyViewModel> GenerateDailyScheduleRange(LoanPaymentSchedulePeriodicViewModel periodicSchedule, DateTime previousPaymentDate)
        //{
        //    List<LoanPaymentScheduleDailyViewModel> output = new List<LoanPaymentScheduleDailyViewModel>();

        //    var dateDifferenceCount = (previousPaymentDate - periodicSchedule.paymentDate).TotalDays;
        //    var currentDate = periodicSchedule.paymentDate;
        //    double previousPrincipalAmount = periodicSchedule.startPrincipalAmount;

        //    for (int counter = 1; counter <= dateDifferenceCount; counter++)
        //    {
        //        LoanPaymentScheduleDailyViewModel dayValues = new LoanPaymentScheduleDailyViewModel();
        //        dayValues.paymentNumber = _dailyScheduleRowCount;
        //        dayValues.paymentDate = periodicSchedule.paymentDate;
        //        dayValues.date = currentDate;

        //        dayValues.openingBalance = previousPrincipalAmount;
        //        dayValues.startPrincipalAmount = periodicSchedule.startPrincipalAmount;
        //        dayValues.dailyPrincipalAmount = periodicSchedule.periodPrincipalAmount / dateDifferenceCount;
        //        dayValues.dailyInterestAmount = periodicSchedule.periodInterestAmount / dateDifferenceCount;
        //        dayValues.dailyPaymentAmount = dayValues.dailyPrincipalAmount + dayValues.dailyInterestAmount;

        ////               public int paymentNumber { get; set; }
        ////public DateTime date { get; set; }
        ////public DateTime paymentDate { get; set; }

        //        //public double openingBalance { get; set; }
        //        //public double startPrincipalAmount { get; set; }
        //        //public double dailyPaymentAmount { get; set; }
        //        //public double dailyInterestAmount { get; set; }
        //        //public double dailyPrincipalAmount { get; set; }
        //        //public double closingBalance { get; set; }
        //        //public double endPrincipalAmount { get; set; }
        //        //public double accruedInterest { get; set; }
        //        //public double amortisedCost { get; set; }
        //        //public double norminalInterestRate { get; set; }

        //        output.Add(dayValues);

        //        previousPrincipalAmount = dayValues.closingBalance;
        //        currentDate = currentDate.AddDays(1);
        //        _dailyScheduleRowCount += 1;
        //    }

        //    return output;

        //}


        private List<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularPeriodicScheduleWithAmortisedCost(LoanPaymentScheduleInputViewModel loanInput)
        {
            if (loanInput.irregularPaymentSchedule.Count() == 0)
                throw new ConditionNotMetException("Specify a repayment schedule");

            if (loanInput.principalAmount != (loanInput.irregularPaymentSchedule.Sum(x => x.paymentAmount)))
                throw new ConditionNotMetException("Payment Amount is not equal to the principal Amount");

            if (loanInput.effectiveDate > (loanInput.irregularPaymentSchedule.Min(x => x.paymentDate)))
                throw new ConditionNotMetException("Effective Date should be less than the payment date(s)");

            List<LoanPaymentSchedulePeriodicViewModel> paymentSchedule = GenerateIrregularPeriodicSchedule(loanInput, false);

            double internalRateOfReturn = 0;

            if (loanInput.integralFeeAmount > 0)
            {
                var amounts = paymentSchedule.Select(x => x.periodPaymentAmount).ToList();  //paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
                var dates = paymentSchedule.Select(x => x.paymentDate).ToList();

                amounts[0] = loanInput.principalAmount * -1;
                internalRateOfReturn = wct.XIRR(amounts, dates, wct.NULL_DOUBLE) * 100;
            }
            else
                internalRateOfReturn = loanInput.interestRate;

            //FinancialTypes.AMORTSCHED_table amortisedResult;
            //amortisedResult = wct.AMORTSCHED(loanInput.principalAmount - loanInput.integralFeeAmount, loanInput.effectiveDate, (internalRateOfReturn / 100.0), loanInput.interestFirstpaymentDate, numberOfPayments, numberOfPaymentsInAYear, daysInAYear, FV, IntRule);

            loanInput.principalAmount = loanInput.principalAmount - loanInput.integralFeeAmount;
            loanInput.interestRate = internalRateOfReturn;  
            List<LoanPaymentSchedulePeriodicViewModel> paymentScheduleArmotised = GenerateIrregularPeriodicSchedule(loanInput, true);

            int counter = 0;
            foreach (var payment in paymentSchedule)
            {
                payment.amortisedStartPrincipalAmount = paymentScheduleArmotised[counter].startPrincipalAmount;
                payment.amortisedPeriodPaymentAmount = paymentScheduleArmotised[counter].periodPaymentAmount;
                payment.amortisedPeriodInterestAmount = paymentScheduleArmotised[counter].periodInterestAmount;
                payment.amortisedPeriodPrincipalAmount = paymentScheduleArmotised[counter].periodPrincipalAmount;
                payment.amortisedEndPrincipalAmount = paymentScheduleArmotised[counter].endPrincipalAmount;
                payment.effectiveInterestRate = internalRateOfReturn;

                counter += 1;
            }

            //if (loanInput.integralFeeAmount > 0)
            //{
            //    var lastRecord = paymentSchedule.Count() - 1;
            //    paymentSchedule[lastRecord].amortisedPeriodPrincipalAmount = paymentSchedule[lastRecord].amortisedPeriodPrincipalAmount - loanInput.integralFeeAmount;
            //    paymentSchedule[lastRecord].amortisedPeriodPaymentAmount = paymentSchedule[lastRecord].amortisedPeriodPaymentAmount - loanInput.integralFeeAmount;
            //    paymentSchedule[lastRecord].amortisedEndPrincipalAmount = paymentSchedule[lastRecord].amortisedStartPrincipalAmount - paymentSchedule[lastRecord].amortisedPeriodPrincipalAmount;
            //}

            return paymentSchedule;
        }


        private List<LoanPaymentSchedulePeriodicViewModel> GenerateIrregularPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput, bool isArmotisedSchedule)
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

            
            double previousPrincipalAmount = loanInput.principalAmount;
            DateTime previousPaymentDate = loanInput.effectiveDate;
            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accrualBasis);

            int paymentNumber = 1;
            foreach (var item in data)
            {
                LoanPaymentSchedulePeriodicViewModel loanPeriod = new LoanPaymentSchedulePeriodicViewModel();
                loanPeriod.paymentNumber = paymentNumber;
                loanPeriod.paymentDate = item.paymentDate;
                loanPeriod.startPrincipalAmount = previousPrincipalAmount;

                if (isArmotisedSchedule == false)
                { loanPeriod.periodPrincipalAmount = item.paymentAmount; }
                else
                {
                    if (loanInput.integralFeeAmount > 0)
                    {
                        var feeDifferential = loanInput.principalAmount / (loanInput.principalAmount + loanInput.integralFeeAmount);
                        loanPeriod.periodPrincipalAmount = item.paymentAmount * feeDifferential;
                    }
                    else
                        loanPeriod.periodPrincipalAmount = item.paymentAmount; 

                }

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
            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accrualBasis);


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
            if (loanInput.effectiveDate == null)
                throw new ConditionNotMetException("Please Enter Effective Date");
            if (loanInput.maturityDate == null)
                throw new ConditionNotMetException("Please Enter Maturity Date");
            if (loanInput.principalAmount <= 0 )
                throw new ConditionNotMetException("Please Enter Loan Amount");
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("Effective Date must be less than the maturity date");

            List<LoanPaymentSchedulePeriodicViewModel> paymentSchedule = GenerateBulletPeriodicSchedule(loanInput);

            double internalRateOfReturn = 0;

            if (loanInput.integralFeeAmount > 0)
            {
                var amounts = paymentSchedule.Select(x => x.periodPaymentAmount).ToList();  //paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
                var dates = paymentSchedule.Select(x => x.paymentDate).ToList();

                amounts[0] = loanInput.principalAmount * -1;
                internalRateOfReturn = wct.XIRR(amounts, dates, wct.NULL_DOUBLE) * 100;
            }
            else
                internalRateOfReturn = loanInput.interestRate;

            loanInput.principalAmount = loanInput.principalAmount - loanInput.integralFeeAmount;
            loanInput.interestRate = internalRateOfReturn;
            List<LoanPaymentSchedulePeriodicViewModel> paymentScheduleArmotised = GenerateBulletPeriodicSchedule(loanInput);

            int counter = 0;
            foreach (var payment in paymentSchedule)
            {
                payment.amortisedStartPrincipalAmount = paymentScheduleArmotised[counter].startPrincipalAmount;
                payment.amortisedPeriodPaymentAmount = paymentScheduleArmotised[counter].periodPaymentAmount;
                payment.amortisedPeriodInterestAmount = paymentScheduleArmotised[counter].periodInterestAmount;
                payment.amortisedPeriodPrincipalAmount = paymentScheduleArmotised[counter].periodPrincipalAmount;
                payment.amortisedEndPrincipalAmount = paymentScheduleArmotised[counter].endPrincipalAmount;
                payment.effectiveInterestRate = internalRateOfReturn;

                counter += 1;
            }

            return paymentSchedule;
        }

        private List<LoanPaymentSchedulePeriodicViewModel> GenerateConstantPrincipalAndInterestPeriodicSchedule(LoanPaymentScheduleInputViewModel loanInput)
        {
            
            int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accrualBasis);

            int numberOfPayments = CalculateNumberOfInstallments(loanInput.interestFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPrincipalPayments = CalculateNumberOfInstallments(loanInput.principalFirstpaymentDate, loanInput.maturityDate, (FrequencyTypeEnum)loanInput.interestFrequency);

            int numberOfPaymentsInAYear = (int)context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == loanInput.interestFrequency).VALUE;

            var principalPaymentsInAYear = (int)context.TBL_FREQUENCY_TYPE.FirstOrDefault(x => x.FREQUENCYTYPEID == loanInput.principalFrequency).VALUE;

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
            if (loanInput.effectiveDate == null)
                throw new ConditionNotMetException("Please Enter Effective Date");
            if (loanInput.maturityDate == null)
                throw new ConditionNotMetException("Please Enter Maturity Date");
            if (loanInput.interestFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Interest Payment Date");
            if (loanInput.principalFirstpaymentDate == null)
                throw new ConditionNotMetException("Please Enter First Principal Payment Date");
            if (loanInput.principalAmount <= 0)
                throw new ConditionNotMetException("Please Enter Loan Amount");
            if (loanInput.effectiveDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("Effective Date must be less than the maturity date");
            if (loanInput.interestFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Interest Payment Date must be less than the maturity date");
            if (loanInput.principalFirstpaymentDate >= loanInput.maturityDate)
                throw new ConditionNotMetException("First Principal Payment Date must be less than the maturity date");
            if (loanInput.effectiveDate > loanInput.principalFirstpaymentDate)
                throw new ConditionNotMetException("First Principal Payment Date cannot be less than the effective date");
            if (loanInput.effectiveDate > loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("First Interest Payment Date cannot be less than the effective date");
            if (loanInput.principalFirstpaymentDate < loanInput.interestFirstpaymentDate)
                throw new ConditionNotMetException("Principal first payment date cannot be less than the interest first payment date");

            List<LoanPaymentSchedulePeriodicViewModel> paymentSchedule = GenerateConstantPrincipalAndInterestPeriodicSchedule(loanInput);

            double internalRateOfReturn = 0;

            if (loanInput.integralFeeAmount > 0)
            {
                var amounts = paymentSchedule.Select(x => x.periodPaymentAmount).ToList();  //paymentSchedule.Where(x => x.paymentNumber > 0).Select(x => x.periodPaymentAmount).ToList();
                var dates = paymentSchedule.Select(x => x.paymentDate).ToList();

                amounts[0] = loanInput.principalAmount * -1;
                internalRateOfReturn = wct.XIRR(amounts, dates, wct.NULL_DOUBLE) * 100;
            }
            else
                internalRateOfReturn = loanInput.interestRate;

            loanInput.principalAmount = loanInput.principalAmount - loanInput.integralFeeAmount;
            loanInput.interestRate = internalRateOfReturn;
            List<LoanPaymentSchedulePeriodicViewModel> paymentScheduleArmotised = GenerateConstantPrincipalAndInterestPeriodicSchedule(loanInput);

            int counter = 0;
            foreach (var payment in paymentSchedule)
            {
                payment.amortisedStartPrincipalAmount = paymentScheduleArmotised[counter].startPrincipalAmount;
                payment.amortisedPeriodPaymentAmount = paymentScheduleArmotised[counter].periodPaymentAmount;
                payment.amortisedPeriodInterestAmount = paymentScheduleArmotised[counter].periodInterestAmount;
                payment.amortisedPeriodPrincipalAmount = paymentScheduleArmotised[counter].periodPrincipalAmount;
                payment.amortisedEndPrincipalAmount = paymentScheduleArmotised[counter].endPrincipalAmount;
                payment.effectiveInterestRate = internalRateOfReturn;

                counter += 1;
            }

            return paymentSchedule;
        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AddLoanSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, int staffId)
        {
            bool output = false;
            var applicationDate = generalSetup.GetApplicationDate();


            //---------------save irregular loan schedule input---------------------------
            List<TBL_LOAN_SCHEDULE_IREGUL_INPUT> tblIrregularSchedule = new List<TBL_LOAN_SCHEDULE_IREGUL_INPUT>();
            LoanScheduleTypeEnum scheduleMethod = (LoanScheduleTypeEnum)loanInput.scheduleMethodId;
            if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
            {
                var data = loanInput.irregularPaymentSchedule.OrderBy(x => x.paymentDate);
                foreach (var item in data)
                {
                    TBL_LOAN_SCHEDULE_IREGUL_INPUT schedule = new TBL_LOAN_SCHEDULE_IREGUL_INPUT();
                    schedule.LOANID = loanId;
                    schedule.PAYMENTDATE = item.paymentDate;
                    schedule.PAYMENTAMOUNT = Convert.ToDecimal(item.paymentAmount);
                    schedule.CREATEDBY = staffId;
                    schedule.DATETIMECREATED = applicationDate;

                    tblIrregularSchedule.Add(schedule);
                }

            }
            //----------------------------------------------


            //----------generate and save periodic loan schedule -----------------------------------
            List<LoanPaymentSchedulePeriodicViewModel> periodicSchedule = GeneratePeriodicLoanSchedule(loanInput);

            List<TBL_LOAN_SCHEDULE_PERIODIC> tblPeriodicSchedule = new List<TBL_LOAN_SCHEDULE_PERIODIC>();
            foreach (var item in periodicSchedule)
            {
                TBL_LOAN_SCHEDULE_PERIODIC schedule = new TBL_LOAN_SCHEDULE_PERIODIC();
                schedule.LOANID = loanId;
                schedule.PAYMENTNUMBER = item.paymentNumber;
                schedule.PAYMENTDATE = item.paymentDate;
                schedule.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                schedule.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                schedule.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                schedule.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                schedule.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                schedule.INTERESTRATE = loanInput.interestRate;

                schedule.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                schedule.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                schedule.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                schedule.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                schedule.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                schedule.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                schedule.CREATEDBY = staffId;
                schedule.DATETIMECREATED = applicationDate;

                tblPeriodicSchedule.Add(schedule);
            }
            //-------------------------------------------------------------------------------------


            //----------generate and save daily loan schedule -----------------------------------
            List<LoanPaymentScheduleDailyViewModel> dailySchedule = GenerateDailyLoanSchedule(loanInput);

            List<TBL_LOAN_SCHEDULE_DAILY> tblDailySchedule = new List<TBL_LOAN_SCHEDULE_DAILY>();

            foreach (var item in dailySchedule)
            {
                TBL_LOAN_SCHEDULE_DAILY schedule = new TBL_LOAN_SCHEDULE_DAILY();

                schedule.LOANID = loanId;
                schedule.PAYMENTNUMBER = item.paymentNumber;
                schedule.DATE = item.date;
                schedule.PAYMENTDATE = item.paymentDate;
                schedule.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                schedule.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                schedule.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                schedule.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                schedule.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                schedule.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                schedule.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                schedule.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                schedule.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                schedule.INTERESTRATE = item.norminalInterestRate;

                schedule.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                schedule.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                schedule.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                schedule.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                schedule.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                schedule.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                schedule.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                schedule.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                schedule.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                schedule.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                schedule.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                schedule.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                schedule.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                schedule.NUMBEROFPERIODS = item.numberOfPeriods;
                schedule.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                schedule.CREATEDBY = staffId;
                schedule.DATETIMECREATED = applicationDate;

                tblDailySchedule.Add(schedule);
            }
            //----------------------------------------------------------------


            //------------adding records to the database--------------------------

            //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
            //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }


            this.context.TBL_LOAN_SCHEDULE_PERIODIC.AddRange(tblPeriodicSchedule);

            this.context.TBL_LOAN_SCHEDULE_DAILY.AddRange(tblDailySchedule);

            //----------update loan details -----------------------------------
            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
            loan.EFFECTIVEDATE = loanInput.effectiveDate;
            loan.MATURITYDATE = periodicSchedule.Max(x => x.paymentDate);
            loan.PRINCIPALNUMBEROFINSTALLMENT = periodicSchedule.Count() -1;
            loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
            loan.OUTSTANDINGPRINCIPAL = (decimal) loanInput.principalAmount;
            loan.OUTSTANDINGINTEREST = (decimal) (periodicSchedule.Select(x => x.periodInterestAmount)).Sum();
            //-------------------------------------------------

            context.SaveChanges();
            //-------------------------------------------------------

            output = true;

            return output;
        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AddLoanFeeSchedule(int loanId, decimal amount, DateTime feeDate, DateTime loanMaturityDate, int feeDay, FrequencyTypeEnum frequency)
        {

            //-----------save recurring fee schedule----------------------------
            var recurringFees = context.TBL_LOAN_FEE.Where(x => x.LOANID == loanId && x.ISRECURRING == true);
            foreach (TBL_LOAN_FEE item in recurringFees)
            {
                var feeInfo = context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == item.CHARGEFEEID).FirstOrDefault();

                var feeSchedule = GenerateFeeSchedule(item.FEEAMOUNT, feeDate, loanMaturityDate, item.RECURRINGPAYMENTDAY, (FrequencyTypeEnum) feeInfo.FEEINTERVALID);

                List<TBL_LOAN_FEE_SCHEDULE> feeScheduleInfo = new List<TBL_LOAN_FEE_SCHEDULE>();

                foreach (var fee in feeSchedule)
                {                    
                    feeScheduleInfo.Add(new TBL_LOAN_FEE_SCHEDULE
                    {
                        FEEAMOUNT = fee.feeAmount,
                        FEEDATE = fee.feeDate,
                        FEENUMBER = fee.paymentNumber
                    });
                }

                this.context.TBL_LOAN_FEE_SCHEDULE.AddRange(feeScheduleInfo);

                context.SaveChanges();
            }
            //------------------------------------------------------------------
            

            return true;
        }


        public List<FeePaymentScheduleViewModel> GenerateFeeSchedule(decimal amount, DateTime feeDate, DateTime loanMaturityDate, int feeDay, FrequencyTypeEnum frequency)
        {

            List<FeePaymentScheduleViewModel> output = new List<FeePaymentScheduleViewModel>();

            int duration = 0;

            if (frequency == FrequencyTypeEnum.Monthly)
                duration = 1;
            else if (frequency == FrequencyTypeEnum.Quarterly)
                duration = 3;
            else if (frequency == FrequencyTypeEnum.ThriceYearly)
                duration = 4;
            else if (frequency == FrequencyTypeEnum.SixTimesYearly)
                duration = 2;
            else if (frequency == FrequencyTypeEnum.TwiceYearly)
                duration = 6;
            else if (frequency == FrequencyTypeEnum.Yearly)
                duration = 12;

            int paymentNumberCount = 1;
  
            var nextPeriodDate = feeDate.AddMonths(duration);

            var nextPayment = new DateTime(nextPeriodDate.Year, nextPeriodDate.Month, feeDay);

                while (nextPayment <= loanMaturityDate)
                {
                    output.Add(new FeePaymentScheduleViewModel
                    {
                        feeAmount = amount,
                        feeDate = nextPayment,
                        paymentNumber = paymentNumberCount
                    });

                    paymentNumberCount += 1;

                    nextPeriodDate = nextPayment.AddMonths(duration);
                    nextPayment = new DateTime(nextPeriodDate.Year, nextPeriodDate.Month, feeDay);
                }
 

            //var startdate = LocalDateTime.FromDateTime(firstPaymentDate);
            //var endDate = LocalDateTime.FromDateTime(maturityDate);

            ////Period period = Period.Between(startdate, endDate, PeriodUnits.Months);

            //double numberOfpayments = 0;

            //if (frequencyType == FrequencyTypeEnum.Daily)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Days).Days;
            //else if (frequencyType == FrequencyTypeEnum.Monthly)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months;
            //else if (frequencyType == FrequencyTypeEnum.Quarterly)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 3.0;
            //else if (frequencyType == FrequencyTypeEnum.SixTimesYearly)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 6.0;
            //else if (frequencyType == FrequencyTypeEnum.ThriceYearly)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 4.0;
            //else if (frequencyType == FrequencyTypeEnum.TwiceMonthly)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months * 2.0;
            //else if (frequencyType == FrequencyTypeEnum.TwiceYearly)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Months).Months / 6.0;
            //else if (frequencyType == FrequencyTypeEnum.Weekly)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Weeks).Weeks;
            //else if (frequencyType == FrequencyTypeEnum.Yearly)
            //    numberOfpayments = Period.Between(startdate, endDate, PeriodUnits.Years).Years;


            //output.Add(new LoanPaymentSchedulePeriodicViewModel
            //{
            //    paymentNumber = 0,
            //    paymentDate = loanInput.effectiveDate,
            //    startPrincipalAmount = 0,
            //    periodPrincipalAmount = 0,
            //    amortisedPeriodInterestAmount = 0,
            //    periodPaymentAmount = 0,
            //    endPrincipalAmount = loanInput.principalAmount
            //});


            //var data = loanInput.irregularPaymentSchedule.OrderBy(x => x.paymentDate);


            //double previousPrincipalAmount = loanInput.principalAmount;
            //DateTime previousPaymentDate = loanInput.effectiveDate;
            //int daysInAYear = GetDaysInAYear((DayCountConventionEnum)loanInput.accurialBasis);

            //int paymentNumber = 1;
            //foreach (var item in data)
            //{
            //    LoanPaymentSchedulePeriodicViewModel loanPeriod = new LoanPaymentSchedulePeriodicViewModel();
            //    loanPeriod.paymentNumber = paymentNumber;
            //    loanPeriod.paymentDate = item.paymentDate;
            //    loanPeriod.startPrincipalAmount = previousPrincipalAmount;

            //    if (isArmotisedSchedule == false)
            //    { loanPeriod.periodPrincipalAmount = item.paymentAmount; }
            //    else
            //    {
            //        if (loanInput.integralFeeAmount > 0)
            //        {
            //            var feeDifferential = loanInput.principalAmount / (loanInput.principalAmount + loanInput.integralFeeAmount);
            //            loanPeriod.periodPrincipalAmount = item.paymentAmount * feeDifferential;
            //        }
            //        else
            //            loanPeriod.periodPrincipalAmount = item.paymentAmount;

            //    }

            //    var dateDifferenceCount = (item.paymentDate - previousPaymentDate).TotalDays;

            //    loanPeriod.periodInterestAmount = (previousPrincipalAmount * (loanInput.interestRate / 100.0)) * (dateDifferenceCount / daysInAYear);
            //    loanPeriod.periodPaymentAmount = loanPeriod.periodPrincipalAmount + loanPeriod.periodInterestAmount;
            //    loanPeriod.endPrincipalAmount = loanPeriod.startPrincipalAmount - loanPeriod.periodPrincipalAmount;

            //    output.Add(loanPeriod);

            //    previousPrincipalAmount = loanPeriod.endPrincipalAmount;
            //    previousPaymentDate = loanPeriod.paymentDate;
            //    paymentNumber += 1;

            //}

            return output;

        }
    }
}
