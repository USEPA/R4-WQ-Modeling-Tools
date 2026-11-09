using NCEIData;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace WeaDB
{
    public class WeaSDB
    {
        private string sdbFile;
        private SqliteConnection conn; // Changed from SQLiteConnection
        private string errmsg;
        private SortedDictionary<string, clsStation> dictSta;
        private SortedDictionary<string, string> dictVar;
        private SortedDictionary<DateTime, double> dictDBSeries;
        private List<string> lstOfPCODES;
        private DateTime dtbeg, dtend;

        public WeaSDB(string _sdbFile)
        {
            this.sdbFile = _sdbFile;
            dictSta = new SortedDictionary<string, clsStation>();
            lstOfPCODES = new List<string>();
            if (!OpenDataBase()) return;
            dictSta = ReadStationsTable();
            lstOfPCODES = ReadPCODETable();
        }
        private bool OpenDataBase()
        {
            try
            {
                string connStr = "Data Source=" + sdbFile;
                conn = new SqliteConnection(connStr); // Changed from SQLiteConnection
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to database " + sdbFile + Environment.NewLine + ex.Message);
                return false;
            }
        }
        public void CloseDataBase()
        {
            conn.Close();
            conn.Dispose();
            conn = null;
        }
        private SortedDictionary<string, clsStation> ReadStationsTable()
        {
            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DISTINCT STATION_ID, STATION_NAME FROM ");
                qry.Append("STATIONS ");
                qry.Append("ORDER BY STATION_ID ");

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string sta = reader["STATION_ID"].ToString();
                        if (!dictSta.ContainsKey(sta))
                        {
                            clsStation csta = new clsStation();
                            csta.STAID = sta;
                            csta.StationName = reader["STATION_NAME"].ToString();
                            dictSta.Add(sta, csta);
                        }
                    }
                }

                return dictSta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private List<string> ReadPCODETable()
        {
            List<string> lstVars = new List<string>();

            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DISTINCT PCODE FROM ");
                qry.Append("PCODES ");
                qry.Append("ORDER BY PCODE ");

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string svar = reader["PCODE"].ToString();
                        if (!lstVars.Contains(svar))
                            lstVars.Add(svar);
                    }
                }

                //debug
                foreach (var kv in lstVars)
                    Debug.WriteLine("PCODE = {0}", kv);
                return lstVars;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool InsertRecordInPCODETable(string svar)
        {
            if (lstOfPCODES.Contains(svar)) return false;
            else
                lstOfPCODES.Add(svar);
            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("INSERT OR REPLACE INTO PCODES");
                qry.Append("(PCode)");
                qry.Append(" VALUES(");
                qry.Append("'" + svar + "')");

                using (var cmd = new SqliteCommand(qry.ToString(), conn)) // Provide command text and connection
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error inserting record in Model table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        public bool InsertRecordInStationTable(string StaID, string StaName, string dset, float lat,
                        float lon, float elev)
        {
            if (dictSta.ContainsKey(StaID)) return false;
            else
            {
                clsStation csta = new clsStation();
                csta.STAID = StaID;
                csta.StationName = StaName;
                csta.Latitude = Convert.ToString(lat);
                csta.Longitude = Convert.ToString(lon);
                csta.Elevation = Convert.ToString(elev);
                dictSta.Add(StaID, csta);
                csta = null;
            }

            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("INSERT OR REPLACE INTO Stations");
                qry.Append("(Station_ID, Station_Name, Latitude," +
                           "Longitude, Elevation)");
                qry.Append(" VALUES(");
                qry.Append("'" + StaID + "',");
                qry.Append("'" + StaName + "',");
                qry.Append("'" + lat + "',");
                qry.Append("'" + lon + "',");
                qry.Append("'" + elev + "')");

                using (var cmd = new SqliteCommand(qry.ToString(), conn)) // Provide command text and connection
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error inserting record in Stations table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        public DataTable SelectRecordsFromMetTable(string tblName, string pcode, string staid)
        {
            DataTable db = new DataTable();
            
            try
            {
                int nrec = 0;
                string begdate, enddate;

                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DATE_TIME, RESULT FROM MET ");
                qry.Append("WHERE STATION_ID = '" + staid + "' AND PCODE = '" + pcode + "' ");
                qry.Append("ORDER BY DATE_TIME ");

                // Create DataTable structure
                db.Columns.Add("DATE_TIME", typeof(string));
                db.Columns.Add("RESULT", typeof(object));

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DataRow row = db.NewRow();
                        row["DATE_TIME"] = reader["DATE_TIME"];
                        row["RESULT"] = reader["RESULT"];
                        db.Rows.Add(row);
                        nrec++;
                    }
                }

                if (nrec == 0) return null;

                begdate = db.Rows[0]["DATE_TIME"].ToString();
                enddate = db.Rows[nrec - 1]["DATE_TIME"].ToString();
                Debug.WriteLine("db count = " + nrec.ToString());
                
                dtbeg = DateTime.Parse(begdate);
                dtend = DateTime.Parse(enddate);
                Debug.WriteLine("{0},{1}", dtbeg.ToString(), dtend.ToString());
            }
            catch (Exception ex)
            {
                errmsg = "Error selecting records in met table!" + Environment.NewLine + ex.Message +
                Environment.NewLine + ex.StackTrace;
                Debug.WriteLine(errmsg);
                return null;
            }
            return db;
        }
        public DateTime BeginRecordDate()
        {
            return dtbeg;
        }
        public DateTime EndingRecordDate()
        {
            return dtend;
        }
        public int GetPeriodOfRecord(string tblName, string pcode, string staid)
        {
            int nrec = 0;
            try
            {
                string begdate = null, enddate = null;

                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DATE_TIME FROM MET ");
                qry.Append("WHERE STATION_ID = '" + staid + "' AND PCODE = '" + pcode + "' ");
                qry.Append("ORDER BY DATE_TIME ");

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string dateTime = reader["DATE_TIME"].ToString();
                        if (begdate == null) begdate = dateTime; // First record
                        enddate = dateTime; // Keep updating to get last record
                        nrec++;
                    }
                }

                if (nrec == 0) return 0;

                dtbeg = DateTime.Parse(begdate);
                dtend = DateTime.Parse(enddate);
            }
            catch (Exception ex)
            {
                errmsg = "Error getting period of record for " + staid + ":" + pcode + "!" + Environment.NewLine + ex.Message +
                Environment.NewLine + ex.StackTrace;
                nrec = 0;
            }
            
            return nrec;
        }
        public SortedDictionary<DateTime, double> FilterRecordsToUpload(DateTime dtbeg, DateTime dtend,
                 SortedDictionary<DateTime, double> dictSeries)
        {
            //dictSeries is he series to upload to sqlite db
            //tblseries is the datatable already in sqlite db
            //datetimes dtbeg and dtend are the period of dtbseries
            SortedDictionary<DateTime, double> dictFilteredSeries = new SortedDictionary<DateTime, double>();
            try
            {
                foreach (var kv in dictSeries)
                {
                    DateTime dt = kv.Key;
                    if (DateTime.Compare(dt, dtbeg) < 0 || DateTime.Compare(dt, dtend) > 0)
                        dictFilteredSeries.Add(dt, kv.Value);
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error filtering records to upload!" + Environment.NewLine + ex.Message +
                Environment.NewLine + ex.StackTrace;
                Debug.WriteLine(errmsg);
                return null;
            }
            return dictFilteredSeries;
        }
        public bool InsertRecordsInMetTable(string tblName, SortedDictionary<DateTime, double> dictSeries,
                                            string pcode, string staid)
        {
            try
            {
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var kv in dictSeries)
                    {
                        DateTime dt = kv.Key;
                        string strdate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        string sval = FormatPCODE(kv.Value, pcode);
                        double dvalue = Convert.ToDouble(sval);

                        StringBuilder qry = new StringBuilder();
                        qry.Append("INSERT OR REPLACE INTO " + tblName);
                        qry.Append("(Station_ID, PCODE, Date_Time, Result)");
                        qry.Append(" VALUES(");
                        qry.Append("'" + staid + "',");
                        qry.Append("'" + pcode + "',");
                        qry.Append("datetime('" + strdate + "'),");
                        qry.Append(dvalue + ")");

                        using (var cmd = new SqliteCommand(qry.ToString(), conn)) // Provide command text and connection
                        {
                            cmd.Transaction = transaction; // Set the transaction
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error inserting record in met table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        public bool DeleteRecordsFromMetTable(string tblName, SortedDictionary<DateTime, double> dictSeries,
                                            string pcode, string staid)
        {
            try
            {
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var kv in dictSeries)
                    {
                        string strdate = kv.Key.ToString("yyyy-MM-dd HH:mm:ss");
                        string sval = FormatPCODE(kv.Value, pcode);
                        double dvalue = Convert.ToDouble(sval);

                        StringBuilder qry = new StringBuilder();
                        qry.Append("DELETE FROM " + tblName);
                        qry.Append(" WHERE ");
                        qry.Append("STATION_ID = '" + staid + "' AND ");
                        qry.Append("PCODE = '" + pcode + "' AND ");
                        qry.Append("DATE_TIME = datetime('" + strdate + "')");

                        using (var cmd = new SqliteCommand(qry.ToString(), conn)) // Provide command text and connection
                        {
                            cmd.Transaction = transaction; // Set the transaction
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error deleting records from " + tblName + " table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private string FormatPCODE(double val, string pcode)
        {
            string sval = string.Empty;
            switch (pcode)
            {
                case "PREC":
                    sval = val.ToString("F3");
                    break;
                case "PRCP":
                    sval = val.ToString("F3");
                    break;
                case "ATEM":
                    sval = val.ToString("F2");
                    break;
                case "TMAX":
                    sval = val.ToString("F2");
                    break;
                case "TMIN":
                    sval = val.ToString("F2");
                    break;
                case "DEWP":
                    sval = val.ToString("F2");
                    break;
                case "SOLR":
                    sval = val.ToString("F5");
                    break;
                case "LRAD":
                    sval = val.ToString("F5");
                    break;
                case "WIND":
                    sval = val.ToString("F3");
                    break;
                case "WNDD":
                    sval = val.ToString("F2");
                    break;
                case "WINDU":
                    sval = val.ToString("F3");
                    break;
                case "WINDV":
                    sval = val.ToString("F3");
                    break;
                case "CLOU":
                    sval = val.ToString("F2");
                    break;
                case "ATMP":
                    sval = val.ToString("F2");
                    break;
                case "PEVT":
                    sval = val.ToString("F5");
                    break;
            }
            return sval;
        }
        public bool TableExist(string tblName)
        {
            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT * FROM sqlite_master WHERE type = 'table'");
                qry.Append("AND tbl_name = '" + tblName + "'");

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error in checking if table " + tblName.ToUpper() + "exist!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public bool CreateTable(string tblName)
        {
            try
            {
                string qry = "CREATE TABLE " + tblName + "(" +
                    "RecID INTEGER NOT NULL," +
                    "Station_ID  VARCHAR(25)," +
                    "Date_Time DATETIME," +
                    "PCode VARCHAR(10)," +
                    "Result    FLOAT," +
                    "Create_Update DATETIME," +
                    "PRIMARY KEY(RecID AUTOINCREMENT)," +
                    "UNIQUE(Station_ID, PCode, Date_Time))";

                using (var cmd = new SqliteCommand(qry, conn)) // Provide command text and connection
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error creating table " + tblName.ToUpper() + "!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
    }
}
