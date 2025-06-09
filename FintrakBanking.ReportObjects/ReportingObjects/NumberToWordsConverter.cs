using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace FintrakBanking.APICore.Results
{
    public class NumberToWordsConverter
    {
        private static readonly string[] Units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
        private static readonly string[] Teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        private static readonly string[] Tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
        private static readonly string[] ThousandsGroups = { "", "Thousand", "Million", "Billion" };

        public static string ConvertAmountToWords(decimal amount)
        {
            if (amount == 0)
                return "Zero Naira";

            var naira = (long)Math.Floor(amount);
            var kobo = (int)((amount - naira) * 100);

            string words = $"{NumberToWords(naira)} Naira";

            if (kobo > 0)
                words += $" and {NumberToWords(kobo)} Kobo";

            return words;
        }

        private static string NumberToWords(long number)
        {
            if (number == 0) return "Zero";

            int groupIndex = 0;
            string words = "";

            while (number > 0)
            {
                int groupValue = (int)(number % 1000);
                if (groupValue != 0)
                {
                    string groupText = GroupToWords(groupValue);
                    if (!string.IsNullOrEmpty(groupText))
                    {
                        words = $"{groupText} {ThousandsGroups[groupIndex]} {words}".Trim();
                    }
                }
                number /= 1000;
                groupIndex++;
            }

            return words.Trim();
        }

        private static string GroupToWords(int number)
        {
            string words = "";

            if (number >= 100)
            {
                words += $"{Units[number / 100]} Hundred ";
                number %= 100;
            }

            if (number >= 10 && number <= 19)
            {
                words += Teens[number - 10] + " ";
            }
            else if (number >= 20)
            {
                words += Tens[number / 10] + " ";
                number %= 10;
            }

            if (number >= 1 && number <= 9)
            {
                words += Units[number] + " ";
            }

            return words.Trim();
        }

        public static string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }

    }
}