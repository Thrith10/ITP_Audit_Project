using System.Data;

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

    }
}