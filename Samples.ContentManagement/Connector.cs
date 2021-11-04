using Newtonsoft.Json;
using Smartsheet.Api;
using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagement
{
    public class Connector
    {
        const string BaseUrl = "https://api.smartsheet.com/2.0/sheets/";
        public const long SheetIdMad = 2312799004190596;
        public const long SheetIdStatt = 6172549672396676;
        public Sheet MadSheet { get; set; }

        // The API identifies columns by Id, but it's more convenient to refer to column names
        public static Dictionary<string, long> ColumnMap;

        public static Sheet InitializeSheet(long sheetId)
        {
            SmartsheetClient smartsheet = new SmartsheetBuilder()
                .SetAccessToken(Secrets.SmartsheetKey)
                .Build();

            Sheet sheet = smartsheet.SheetResources.GetSheet(
              sheetId,                    // sheetId
              null,                       // IEnumerable<SheetLevelInclusion> includes
              null,                       // IEnumerable<SheetLevelExclusion> excludes
              null,                       // IEnumerable<long> rowIds
              null,                       // IEnumerable<int> rowNumbers
              null,                       // IEnumerable<long> columnIds
              null,                       // Nullable<long> pageSize
              null                        // Nullable<long> page
            );

            // Instantiate a new column map every time the Sheet is initialized
            ColumnMap = new Dictionary<string, long>();

            // Build column map for later reference
            foreach (Column column in sheet.Columns)
                ColumnMap.Add(column.Title, (long)column.Id);

            return sheet;
        }

        public static List<Row> GetRows(Sheet sheet)
        {
            var rowsOut = new List<Row>();

            foreach (Row r in sheet.Rows)
            {
                rowsOut.Add(r);
            }

            return rowsOut;
        }

        /// <summary>
        /// Helper method to get the ID of a Column by its name.
        /// </summary>
        /// <param name="sheet">The Sheet to search through.</param>
        /// <param name="name">The name of the Column.</param>
        /// <returns>The ID of the Column.</returns>
        public static long? GetColumnIdByName(Sheet sheet, string name)
        {
            long? colId = null;

            foreach (var c in sheet.Columns)
            {
                if (c.Title.ToLower() == name.ToLower())
                {
                    colId = c.Id;
                }
            }

            return colId;
        }

        /// <summary>
        /// Get the value of a specific Cell on a Sheet.
        /// </summary>
        /// <param name="row">The Row object that the Cell belongs to.</param>
        /// <param name="colId">The ID of the Column that the Cell belongs to.</param>
        /// <returns>The value of a Cell formatted as a string.</returns>
        public static string GetCellValue(Row row, long? colId)
        {
            // Return value
            string value = String.Empty;

            if (row != null)
            {
                // Loop through all Cells within Row to find the current Column
                foreach (var cell in row.Cells)
                {
                    // If Column found by ID and the value is not null, return the value of the Cell
                    if (cell.ColumnId == colId && cell.Value != null)
                    {
                        // Set the value of value to return
                        value = cell.Value.ToString();
                    }
                }
            }

            return value;
        }
    }
}
