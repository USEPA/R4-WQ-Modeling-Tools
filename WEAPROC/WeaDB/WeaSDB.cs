using NCEIData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace WeaDB
{
    public class WeaSDB
    {
        private string sdbFile;
        private SQLiteConnection conn;
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
                conn = new SQLiteConnection(connStr);
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
            DataTable dbSta = new DataTable();

            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DISTINCT STATION_ID, STATION_NAME FROM ");
                qry.Append("STATIONS ");
                qry.Append("ORDER BY STATION_ID ");

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(qry.ToString(), conn);
                adapter.Fill(dbSta);
                qry = null;
                adapter = null;

                foreach (DataRow dr in dbSta.Rows)
                {
                    string sta = dr["STATION_ID"].ToString();
                    if (!dictSta.ContainsKey(sta))
                    {
                        clsStation csta = new clsStation();
                        csta.STAID = sta;
                        csta.StationName = dr["STATION_NAME"].ToString();
                        dictSta.Add(sta, csta);
                        csta = null;
                    }
                }
                dbSta = null;

                //debug
                //foreach (var kv in dictSta)
                //    Debug.WriteLine("{0},{1}", kv.Key, kv.Value);
                return dictSta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private List<string> ReadPCODETable()
        {
            DataTable db = new DataTable();
            List<string> lstVars = new List<string>();

            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DISTINCT PCODE FROM ");
                qry.Append("PCODES ");
                qry.Append("ORDER BY PCODE ");

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(qry.ToString(), conn);
                adapter.Fill(db);
                qry = null;
                adapter = null;

                foreach (DataRow dr in db.Rows)
                {
                    string svar = dr["PCODE"].ToString();
                    if (!lstVars.Contains(svar))
                        lstVars.Add(svar);
                }
                db = null;

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
                var cmd = new SQLiteCommand(conn);

                StringBuilder qry = new StringBuilder();
                qry.Append("INSERT OR REPLACE INTO PCODES");
                qry.Append("(PCode)");
                qry.Append(" VALUES(");
                qry.Append("'" + svar + "')");

                cmd.CommandText = qry.ToString();
                cmd.ExecuteNonQuery();
                qry = null;
                cmd = null;
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
                var cmd = new SQLiteCommand(conn);

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

                cmd.CommandText = qry.ToString();
                cmd.ExecuteNonQuery();
                qry = null;
                cmd = null;
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
            //call to delete table in main is disabled, need to check if records are in
            try
            {
                int nrec = 0;
                string begdate, enddate;

                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DATE_TIME, RESULT FROM MET ");
                qry.Append("WHERE STATION_ID = '" + staid + "' AND PCODE = '" + pcode + "' ");
                qry.Append("ORDER BY DATE_TIME ");

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(qry.ToString(), conn);
                adapter.Fill(db);
                qry = null;
                adapter = null;

                if ((nrec = db.Rows.Count) == 0) return null;

                begdate = db.Rows[0]["DATE_TIME"].ToString();
                enddate = db.Rows[nrec - 1]["DATE_TIME"].ToString();
                Debug.WriteLine("db count = " + nrec.ToString());
                //Debug.WriteLine("{0},{1}", begdate, enddate);
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
            DataTable db = new DataTable();
            //call to delete table in main is disabled, need to check if records are in
            int nrec = 0;
            try
            {
                string begdate, enddate;

                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DATE_TIME FROM MET ");
                qry.Append("WHERE STATION_ID = '" + staid + "' AND PCODE = '" + pcode + "' ");
                qry.Append("ORDER BY DATE_TIME ");

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(qry.ToString(), conn);
                adapter.Fill(db);
                qry = null;
                adapter = null;

                if ((nrec = db.Rows.Count) == 0)
                {
                    db = null; return 0;
                }

                //if nnrec> -0
                begdate = db.Rows[0]["DATE_TIME"].ToString();
                enddate = db.Rows[nrec - 1]["DATE_TIME"].ToString();
                //Debug.WriteLine("db count = " + nrec.ToString());
                //Debug.WriteLine("{0},{1}", begdate, enddate);
                dtbeg = DateTime.Parse(begdate);
                dtend = DateTime.Parse(enddate);
                //Debug.WriteLine("{0},{1}", dtbeg.ToString(), dtend.ToString());
            }
            catch (Exception ex)
            {
                errmsg = "Error getting period of record for " + staid + ":" + pcode + "!" + Environment.NewLine + ex.Message +
                Environment.NewLine + ex.StackTrace;
                //Debug.WriteLine(errmsg);
                nrec = 0;
            }
            db = null;
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
            string strdate, sval;
            double dvalue;
            try
            {
                // Insert data
                //dtbeg = DateTime.Parse(begdate);
                //dtend = DateTime.Parse(enddate);

                var cmd = new SQLiteCommand(conn);
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var kv in dictSeries)
                    {

                        DateTime dt = kv.Key;
                        strdate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        sval = FormatPCODE(kv.Value, pcode);
                        dvalue = Convert.ToDouble(sval);

                        StringBuilder qry = new StringBuilder(string.Empty);
                        //INSERT OR REPLACE RECORD
                        //StringBuilder qry = new StringBuilder();
                        qry.Clear();
                        qry.Append("INSERT OR REPLACE INTO " + tblName);
                        qry.Append("(Station_ID, PCODE, Date_Time, Result)");
                        qry.Append(" VALUES(");
                        qry.Append("'" + staid + "',");
                        qry.Append("'" + pcode + "',");
                        qry.Append("datetime('" + strdate + "'),");
                        qry.Append(dvalue + ")");

                        cmd.CommandText = qry.ToString();
                        cmd.ExecuteNonQuery();
                        qry = null;
                    }
                    transaction.Commit();
                }
                cmd = null;
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
            string strdate, sval;
            double dvalue;
            try
            {
                var cmd = new SQLiteCommand(conn);
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var kv in dictSeries)
                    {

                        strdate = kv.Key.ToString("yyyy-MM-dd HH:mm:ss");
                        sval = FormatPCODE(kv.Value, pcode);
                        dvalue = Convert.ToDouble(sval);

                        StringBuilder qry = new StringBuilder(string.Empty);
                        qry.Append("DELETE FROM " + tblName);
                        qry.Append(" WHERE ");
                        qry.Append("STATION_ID = '" + staid + "' AND ");
                        qry.Append("PCODE = '" + pcode + "' AND ");
                        qry.Append("DATE_TIME = datetime('" + strdate + "')");

                        cmd.CommandText = qry.ToString();
                        cmd.ExecuteNonQuery();
                        qry = null;
                    }
                    transaction.Commit();
                }
                cmd = null;
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
            //SELECT sql FROM sqlite_master WHERE type = 'table' AND tbl_name = 'COMPANY';
            try
            {
                DataTable db = new DataTable();
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT * FROM sqlite_master WHERE type = 'table'");
                qry.Append("AND tbl_name = " + "'" + tblName + "'");
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(qry.ToString(), conn);
                adapter.Fill(db);
                qry = null;
                adapter = null;

                if (db.Rows.Count > 0)
                    return true;
                else
                    return false;
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
                //in WRDB create
                //CREATE TABLE "Met"("RecID" INTEGER NOT NULL, "Station_ID" VARCHAR(25), 
                //"Date_Time" DATETIME, "PCode" VARCHAR(10), "LEW_Pct" FLOAT, "Depth_M" FLOAT, 
                //"Agency" VARCHAR(5), "CCode" CHAR(3), "LCode" CHAR(3), "SCode" CHAR(3), 
                //"QCode" CHAR(1), "RCode" CHAR(1), "Result" FLOAT, "Validated" BOOL, 
                //"Create_Update" DATETIME, "Owner" CHAR(3), "Track_ID" INT, 
                //PRIMARY KEY("RecID" AUTOINCREMENT), UNIQUE("Station_ID", "PCode", "Date_Time"))

                //simplified
                //CREATE TABLE "Met"(
                //"RecID" INTEGER NOT NULL,
                //"Station_ID"    VARCHAR(25) UNIQUE,
                //"Date_Time" DATETIME UNIQUE,
                //"PCode" VARCHAR(10) UNIQUE,
                //"Result"    FLOAT,
                //"Create_Update" DATETIME,
                //UNIQUE("Station_ID", "PCode", "Date_Time"),
                //PRIMARY KEY("RecID" AUTOINCREMENT))

                string qry = "CREATE TABLE " + tblName + "(" +
                    "RecID INTEGER NOT NULL," +
                    "Station_ID  VARCHAR(25)," +
                    "Date_Time DATETIME," +
                    "PCode VARCHAR(10)," +
                    "Result    FLOAT," +
                    "Create_Update DATETIME," +
                    "PRIMARY KEY(RecID AUTOINCREMENT)," +
                    "UNIQUE(Station_ID, PCode, Date_Time))";

                var cmd = new SQLiteCommand(conn);
                cmd.CommandText = qry.ToString();
                cmd.ExecuteNonQuery();
                qry = null;
                cmd = null;
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
