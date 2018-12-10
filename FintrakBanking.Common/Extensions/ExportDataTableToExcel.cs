
using FintrakBanking.Common.CustomException;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FintrakBanking.Common.Extensions
{
    public class ExportDataTableToExcel
    {
        public void CreateHtmlTable(DataTable table)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            HttpContext.Current.Response.Write(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=Reports.xls");

            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
            //sets font
            HttpContext.Current.Response.Write("<font style='font-size:10.0pt; font-family:Calibri;'>");
            HttpContext.Current.Response.Write("<BR><BR><BR>");
            //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
            HttpContext.Current.Response.Write("<Table border='1' bgColor='#ffffff' " +
              "borderColor='#000000' cellSpacing='0' cellPadding='0' " +
              "style='font-size:10.0pt; font-family:Calibri; background:white;'> <TR>");
            //am getting my grid's column headers
            int columnscount = table.Columns.Count;

            for (int j = 0; j < columnscount; j++)
            {      //write in new column
                HttpContext.Current.Response.Write("<Td>");
                //Get column headers  and make it as bold in excel columns
                HttpContext.Current.Response.Write("<B>");
                HttpContext.Current.Response.Write(table.Columns[j].ColumnName.ToString());
                HttpContext.Current.Response.Write("</B>");
                HttpContext.Current.Response.Write("</Td>");
            }
            HttpContext.Current.Response.Write("</TR>");
            foreach (DataRow row in table.Rows)
            {//write in new row
                HttpContext.Current.Response.Write("<TR>");
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    HttpContext.Current.Response.Write("<Td>");
                    HttpContext.Current.Response.Write(row[i].ToString());
                    HttpContext.Current.Response.Write("</Td>");
                }

                HttpContext.Current.Response.Write("</TR>");
            }
            HttpContext.Current.Response.Write("</Table>");
            HttpContext.Current.Response.Write("</font>");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }
        [STAThread]
        public void ExportToExcel(DataTable dt)
        {
            // If using Professional version, put your serial key below.
            //SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            //ExcelFile ef = new ExcelFile();
            //ExcelWorksheet ws = ef.Worksheets.Add("DataTable to Sheet");

         
            //// Insert DataTable into an Excel worksheet.
            //ws.InsertDataTable(dt,
            //    new InsertDataTableOptions()
            //    {
            //        ColumnHeaders = true,
            //        StartRow = 2
            //    });

            //using (FileStream stream = new FileStream("C:\\Users\\uuser\\Downloads\\Sheet.xlsx", FileMode.CreateNew))
            //{
            //    // Saves file to the stream
            //    ef.Save(stream, SaveOptions.PdfDefault);
            //}
           // ef.Save(this.Response, "C:\\Users\\uuser\\Downloads\\DataTable to Sheet.xlsx");
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }

            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }

                dataTable.Rows.Add(values);

            }
            //put a breakpoint here and check datatable
            return dataTable;
        }


        public bool Export2Excel(DataTable dataTable)
        {
           
            object misValue = System.Reflection.Missing.Value;

            Microsoft.Office.Interop.Excel.Application _appExcel = null;
            Microsoft.Office.Interop.Excel.Workbook _excelWorkbook = null;
            Microsoft.Office.Interop.Excel.Worksheet _excelWorksheet = null;
            try
            {

                if (dataTable.Rows.Count <= 0) { throw new ArgumentNullException("Table is Empty"); }

                // excel app object
                _appExcel = new Microsoft.Office.Interop.Excel.Application();

                // excel workbook object added to app
                _excelWorkbook = _appExcel.Workbooks.Add(misValue);
                _excelWorksheet = _appExcel.ActiveWorkbook.ActiveSheet as Microsoft.Office.Interop.Excel.Worksheet;


                // column names row (range obj)
                Microsoft.Office.Interop.Excel.Range _columnsNameRange;
                _columnsNameRange = _excelWorksheet.get_Range("A1", misValue).get_Resize(1, dataTable.Columns.Count);

                // column names array to be assigned to _columnNameRange
                string[] _arrColumnNames = new string[dataTable.Columns.Count];

                // set Excel columns NumberFormat property
                // note; most important for decimal-currency, DateTime
                for (int i = 4; i < dataTable.Columns.Count + 5; i++)
                {
                    // array of column names
                    _arrColumnNames[i] = dataTable.Columns[i].ColumnName;

                    string _strType = dataTable.Columns[i].DataType.FullName.ToString();
                    switch (_strType)
                    {
                        case "System.DateTime":
                            {
                                _excelWorksheet.Range["A1"].Offset[misValue, i].EntireColumn.NumberFormat = "MM/DD/YY";
                                break;
                            }
                        case "System.Decimal":
                            {
                                _excelWorksheet.Columns["A"].Offset[misValue, i].EntireColumn.NumberFormat = "$ #,###.00";
                                break;
                            }
                        case "System.Double":
                            {
                                _excelWorksheet.Columns["A"].Offset[misValue, i].EntireColumn.NumberFormat = "#.#";
                                break;
                            }
                        case "System.Int8":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.Int64":
                            {
                                // use general format for int
                                //_excelWorksheet.Columns["A"].Offset[misValue, i].EntireColumn.NumberFormat = "####";
                                break;
                            }
                        default: break;
                    }
                }

                //_excelWorksheet.Rows.Insert(1);
                //_excelWorksheet.Rows.Insert(2);
                //_excelWorksheet.Rows.Insert(3);
                //_excelWorksheet.Rows.Insert(4);

                // assign array to column headers range, make 'em bold
                _columnsNameRange.set_Value(misValue, _arrColumnNames);
                _columnsNameRange.Font.Bold = true;

                // populate data content row by row
                for (int Idx = 0; Idx < dataTable.Rows.Count; Idx++)
                {
                    _excelWorksheet.Range["A2"].Offset[Idx].Resize[1, dataTable.Columns.Count].Value =
                    dataTable.Rows[Idx].ItemArray;
                }

                // Autofit all Columns in the range
                _columnsNameRange.Columns.EntireColumn.AutoFit();

                // quit excel app process
                if (_appExcel != null)
                {
                    _appExcel.UserControl = false;
                    _appExcel.Quit();
                }
                return true;
            }
            catch(Exception ex) {

                throw new SecureException("" +ex);
            }
            finally
            {
                _excelWorksheet = null;
                _excelWorkbook = null;
                _appExcel = null;
                misValue = null;
            }
        }

    }
}
