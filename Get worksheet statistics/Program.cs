using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;


namespace Get_worksheet_statistics
{
    class Program
    {
        static void Main(string[] args)
        {
            int i, j, index;
            double fValue;
            Random rand = new Random();
            string[] stringdata = new string[] { "Apple", "Banana", "Cherry", "Durian", "Elderberry" };

            using (SLDocument sl = new SLDocument())
            {
                sl.RenameWorksheet(SLDocument.DefaultFirstSheetName, "LINQ");

                for (i = 1; i <= 16; ++i)
                {
                    for (j = 1; j <= 6; ++j)
                    {
                        switch (rand.Next(6))
                        {
                            case 0:
                            case 1:
                                sl.SetCellValue(i, j, stringdata[rand.Next(stringdata.Length)]);
                                break;
                            case 2:
                            case 3:
                                sl.SetCellValue(i, j, rand.NextDouble() * 1000.0 + 350.0);
                                break;
                            case 4:
                                sl.SetCellValue(i, j, rand.Next(2) == 1 ? true : false);
                                break;
                            case 5:
                                if (rand.NextDouble() < 0.5)
                                {
                                    sl.SetCellValueNumeric(i, j, "3.1415926535898");
                                }
                                else
                                {
                                    sl.SetCellValueNumeric(i, j, "2.7182818284590");
                                }
                                break;
                        }
                    }
                }

                //Dictionary<SLCellPoint, SLCell> cells = sl.GetCells();
                Dictionary<SLCellPoint, SLCell> cells = sl.GetCells();//应该如何处理？


                sl.SetCellValue("I2", string.Format("There are {0} boolean values",
                    cells.Where(bl => bl.Value.DataType == CellValues.Boolean).Count()));

                SLCell cell;

                sl.SetCellValue("I4", "Values below 500 (CellRefs)");
                i = 5; // start from row 5
                foreach (var kvp in cells.Where(num => num.Value.DataType == CellValues.Number))
                {
                    // The Key of the dictionary is SLCellPoint
                    cell = cells[kvp.Key];
                    if (cell.CellText != null)
                    {
                        // note that PI or E is stored in CellText even though it's a number,
                        // via the SetCellValueNumeric() function.
                        // This is in case you want to force-store a number you want that's in
                        // a string form, and/or you want it stored *exactly* as it is in Excel.
                        // This is why it's complicated.
                        // This is also why it's not encouraged to get values using the SLCell object.
                        // SLCell had a scope originally as internal (or *not public*).
                        // You've been warned. Have fun though!
                        fValue = Convert.ToDouble(cell.CellText);
                    }
                    else
                    {
                        // if CellText is null, then the NumericValue is used.
                        fValue = cell.NumericValue;
                    }

                    if (fValue < 500)
                    {
                        sl.SetCellValue(i, 9, SLConvert.ToCellReference(kvp.Key.RowIndex, kvp.Key.ColumnIndex));
                        ++i;
                    }
                }

                List<SLRstType> richtextlist = sl.GetSharedStrings();
                index = -1;
                for (i = 0; i < richtextlist.Count; ++i)
                {
                    // You'd use richtextlist[i].ToPlainString() to get the whole thing
                    // but we "know" the rich text is just plain text.
                    if (richtextlist[i].GetText().Equals("Apple"))
                    {
                        index = i;
                        break;
                    }
                }

                if (index > -1)
                {
                    sl.SetCellValue("L4", "Apples at:");
                    i = 5; // start at row 5
                    foreach (var kvp in cells.Where(apple => apple.Value.DataType == CellValues.SharedString &&
                        Convert.ToInt32(apple.Value.NumericValue) == index))
                    {
                        sl.SetCellValue(i, 12, SLConvert.ToCellReference(kvp.Key.RowIndex, kvp.Key.ColumnIndex));
                        ++i;
                    }
                }
                else
                {
                    sl.SetCellValue("L4", "There are no apples :(");
                }

                sl.AddWorksheet("Teleport");

                for (i = 0; i < 5; ++i)
                {
                    // "restrict" to rows 3 to 20, columns 3 to 10
                    sl.SetCellValue(rand.Next(3, 21), rand.Next(3, 11), "Teleport");
                }

                SLWorksheetStatistics wsstats = sl.GetWorksheetStatistics();
                sl.SetCellValue("A1", string.Format("There are {0} cells, {1} rows and {2} columns set.",
                    wsstats.NumberOfCells, wsstats.NumberOfRows, wsstats.NumberOfColumns));
                sl.SetCellValue("A2", string.Format("Teleport range: Rows {0} to {1}, Columns {2} to {3}",
                    wsstats.StartRowIndex, wsstats.EndRowIndex, wsstats.StartColumnIndex, wsstats.EndColumnIndex));

                sl.AddWorksheet("Measure");

                sl.SetColumnWidth(3, 15);

                // It's English Metric Units, not the bird... Emu它是英国的公制单位。
                sl.SetCellValue("E6", sl.GetWidth(SLMeasureUnitTypeValues.Emu, 2));
                sl.SetCellValue("E7", sl.GetWidth(SLMeasureUnitTypeValues.Emu, 3));
                // here we get the sum of widths of columns 2 and 3.
                // The previous statements are to show you the actual widths first.
                sl.SetCellValue("E8", sl.GetWidth(SLMeasureUnitTypeValues.Emu, 2, 3));
                // In case you hate emus...
                sl.SetCellValue("E9", sl.GetWidth(SLMeasureUnitTypeValues.Inch, 2, 3));

                // you can do the same for row heights.
                // This gives you the combined row heights of rows 5 to 8 in points.
                sl.SetCellValue("E12", sl.GetHeight(SLMeasureUnitTypeValues.Point, 5, 8));

                // just so when we open the spreadsheet, it's on this worksheet.
                // Yes, you don't have to do anything special other than select it.
                // Just like in Excel!
                sl.SelectWorksheet("LINQ");

                // alright fine, let's at least do *something*...
                sl.SetActiveCell("E7");//设置E7为当前工作表的活动单元

                sl.SaveAs("Miscellaneous.xlsx");
            }

            Console.WriteLine("End of program");
            Console.ReadLine();
        }
    }
}
