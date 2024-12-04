using System.Data;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Dapper;
using Npgsql;


namespace PKFAuditManagement.Util
{
    public class DataHelper
    {
        public static DataTable ConvertToDataTable(IEnumerable<dynamic> data)
        {
            var dataTable = new DataTable();
            if (data.Any())
            {
                var firstRow = (IDictionary<string, object>)data.First();
                foreach (var column in firstRow.Keys)
                {
                    dataTable.Columns.Add(column);
                }

                foreach (var row in data)
                {
                    var dataRow = dataTable.NewRow();
                    var rowDict = (IDictionary<string, object>)row;
                    foreach (var key in rowDict.Keys)
                    {
                        dataRow[key] = rowDict[key] ?? DBNull.Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }
            return dataTable;
        }

        // Execute query and add result to DataTables list
        public static async Task ExecuteQueryAndAddToDataTablesAsync(string query, NpgsqlConnection connection, List<DataTable> dataTables, object parameters)
        {
            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    var result = await connection.QueryAsync<dynamic>(query, parameters);
                    dataTables.Add(DataHelper.ConvertToDataTable(result));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing query: {query}\nMessage: {ex.Message}");
                    throw;
                }
            }
        }

        // Filter DataTable based on column name and filter value, return new DataTable with filtered rows or empty DataTable
        public static DataTable FilterDataTable(DataTable dataTable, string columnName, string filter)
        {
            var filteredRows = dataTable.AsEnumerable()
                .Where(row => row.Field<string>(columnName) == filter)
                .ToList();

            return filteredRows.Any() ? filteredRows.CopyToDataTable() : dataTable.Clone();
        }

        // Add headers to worksheet from DataTable columns
        public static void AddHeadersToWorksheet(IXLWorksheet worksheet, DataTable dataTable, int startRow, int startColumn)
        {
            var headers = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
            for (int i = 0; i < headers.Count; i++)
            {
                // Set header value
                var cell = worksheet.Cell(startRow, startColumn + i);
                cell.Value = headers[i];

                // Make header bold
                cell.Style.Font.Bold = true;
            }
        }

        // Add data to worksheet from DataTable rows
        public static void AddDataToWorksheet(IXLWorksheet worksheet, DataTable dataTable, int startRow, int startColumn)
        {
            int currentRow = startRow;
            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    var value = row.ItemArray[i];
                    worksheet.Cell(currentRow, startColumn + i).Value = value == DBNull.Value || value == null ? "Null" : value.ToString();
                }
                currentRow++;
            }
        }

        public static DataTable HandleEmptyDataTable(DataTable dataTable, string placeholderColumnName, string placeholderMessage)
        {
            // Ensure table is empty but with the correct schema
            dataTable.Clear();
            var placeholderRow = dataTable.NewRow();

            foreach (DataColumn column in dataTable.Columns)
            {
                placeholderRow[column] = column.ColumnName == placeholderColumnName ? placeholderMessage : "N/A";
            }

            dataTable.Rows.Add(placeholderRow);
            return dataTable;
        }
    }
}