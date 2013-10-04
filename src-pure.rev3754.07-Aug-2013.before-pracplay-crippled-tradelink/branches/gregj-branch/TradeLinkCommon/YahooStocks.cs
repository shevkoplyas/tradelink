using System;
using System.Data;
using System.Net;
//using System.Data.SQLite;

namespace TradeLink.Common
{
    public class YahooDownloader
    {
        private string urlTemplate =
            @"http://ichart.finance.yahoo.com/table.csv?s=[symbol]&a=" +
              "[startMonth]&b=[startDay]&c=[startYear]&d=[endMonth]&e=" +
                 "[endDay]&f=[endYear]&g=d&ignore=.csv";

        public DataTable UpdateSymbol(string symbol, DateTime? startDate, DateTime? endDate)
        {
            if (!endDate.HasValue) endDate = DateTime.Now;
            if (!startDate.HasValue) startDate = DateTime.Now.AddYears(-5);
            if (symbol == null || symbol.Length < 1)
                throw new ArgumentException("Symbol invalid: " + symbol);
            // NOTE: Yahoo's scheme uses a month number 1 less than actual e.g. Jan. ="0"
            int strtMo = startDate.Value.Month - 1;
            string startMonth = strtMo.ToString();
            string startDay = startDate.Value.Day.ToString();
            string startYear = startDate.Value.Year.ToString();

            int endMo = endDate.Value.Month - 1;
            string endMonth = endMo.ToString();
            string endDay = endDate.Value.Day.ToString();
            string endYear = endDate.Value.Year.ToString();

            urlTemplate = urlTemplate.Replace("[symbol]", symbol);

            urlTemplate = urlTemplate.Replace("[startMonth]", startMonth);
            urlTemplate = urlTemplate.Replace("[startDay]", startDay);
            urlTemplate = urlTemplate.Replace("[startYear]", startYear);

            urlTemplate = urlTemplate.Replace("[endMonth]", endMonth);
            urlTemplate = urlTemplate.Replace("[endDay]", endDay);
            urlTemplate = urlTemplate.Replace("[endYear]", endYear);
            string history = String.Empty;
            WebClient wc = new WebClient();
            try
            {
                history = wc.DownloadString(urlTemplate);
            }
            catch (WebException wex)
            {
                //  throw wex;
            }
            finally
            {
                wc.Dispose();
            }
            DataTable dt = new DataTable();
            // trim off unused characters from end of line
            history = history.Replace("\r", "");
            // split to array on end of line
            string[] rows = history.Split('\n');
            // split to colums
            string[] colNames = rows[0].Split(',');
            // add the columns to the DataTable
            foreach (string colName in colNames)
                dt.Columns.Add(colName);
            DataRow row = null;
            string[] rowValues;
            object[] rowItems;
            // split the rows
            for (int i = rows.Length - 1; i > 0; i--)
            {
                rowValues = rows[i].Split(',');
                row = dt.NewRow();
                rowItems = ConvertStringArrayToObjectArray(rowValues);
                if (rowItems[0] != null && (string)rowItems[0] != "")
                {
                    row.ItemArray = rowItems;
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }


        //public void InsertOrUpdateIssue(DataTable issueTable, string symbol)
        //{
        //    if (issueTable.Rows.Count == 0) return;
        //    symbol = symbol.Replace("^", "");
        //    string InsertMasterSQL = Constants.NewSymbolSQLTemplate;
        //    //"REPLACE INTO MASTER (SYMBOL,FIRSTDATE,LASTDATE) VALUES (@Symbol, @FirstDate, @LastDate)";

        //    DateTime FirstDate = Convert.ToDateTime(issueTable.Rows[0]["Date"]);
        //    DateTime LastDate = Convert.ToDateTime(issueTable.Rows[issueTable.Rows.Count - 1]["Date"]);
        //    object[] parms = { symbol, FirstDate, LastDate };
        //    SQLiteHelper.ExecuteNonQuery(Constants.ConnectionString, InsertMasterSQL, parms);
        //    string createIssueTableSql = Constants.SymbolSQLTemplate;
        //    // "CREATE TABLE [Data](Symbol VARCHAR(50) NOT NULL,Date DATETIME NOT NULL,
        //    // Open FLOAT,High FLOAT NOT NULL, Low FLOAT NOT NULL, Close FLOAT NOT NULL, Volume INTEGER)";
        //    createIssueTableSql = createIssueTableSql.Replace("[Data]", symbol);
        //    try
        //    {
        //        SQLiteHelper.ExecuteNonQuery(Constants.ConnectionString, createIssueTableSql, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        // TABLE ALREADY EXISTS (Sorry to use exception for biz logic, but hey...)
        //    }
        //    SQLiteConnection cn = new SQLiteConnection(Constants.ConnectionString);
        //    cn.Open();
        //    // always do multiple operations in SQLite in a transaction!
        //    SQLiteTransaction trans = cn.BeginTransaction();
        //    //"INSERT INTO [SYMBOL] (SYMBOL,DATE,OPEN,HIGH,LOW,CLOSE,VOLUME) 
        //    // VALUES(@Symbol,@Date,@Open,@High,@Low,@Close,@Volume)"
        //    string sql = Constants.InsertSymbolSQLTemplate;
        //    foreach (DataRow row in issueTable.Rows)
        //    {
        //        string sym = symbol;
        //        DateTime date = Convert.ToDateTime(row["Date"]);
        //        float open = (float)Convert.ToDouble(row["Open"]);
        //        float high = (float)Convert.ToDouble(row["High"]);
        //        float low = (float)Convert.ToDouble(row["Low"]);
        //        float close = (float)Convert.ToDouble(row["close"]);
        //        double volume = Convert.ToDouble(row["Volume"]);
        //        object[] parms2 = { sym, date, open, high, low, close, volume };
        //        sql = sql.Replace("[SYMBOL]", symbol);
        //        SQLiteHelper.ExecuteNonQuery(trans, sql, parms2);
        //    }
        //    trans.Commit();  //using a transaction for SQLite speeds up multi-inserts bigtime!
        //    cn.Close();
        //}

        private object[] ConvertStringArrayToObjectArray(string[] input)
        {
            int elements = input.Length;
            object[] objArray = new object[elements];
            input.CopyTo(objArray, 0);
            return objArray;
        }
    }
}