using FintrakBanking.ViewModels.Credit;
using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Helper
{
   public class DataExport
    {
        [STAThread]
        public void ExportToExcel(DataTable dt, List<LoanPaymentSchedulePeriodicViewModel> data)
        {
            // If using Professional version, put your serial key below.
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            ExcelFile ef = new ExcelFile();
            ExcelWorksheet ws = ef.Worksheets.Add("DataTable to Sheet");

            var file = data.FirstOrDefault();

            ws.Cells[0, 0].Value = "Principal Amount : " + file.startPrincipalAmount;
            ws.Cells[1, 0].Value = "Effective Date : " + file.effectiveInterestRate;
 
            // Insert DataTable into an Excel worksheet.
            ws.InsertDataTable(dt,
                new InsertDataTableOptions()
                {
                    ColumnHeaders = true,
                    StartRow = 2
                });

            ef.Save("C:\\Users\\uuser\\Downloads\\DataTable to Sheet.xlsx");
        }
    }
}
