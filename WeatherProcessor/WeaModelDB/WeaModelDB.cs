using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace WeaModelDB
{
    public class ModelDB
    {
        private string sdbFile;
        private SQLiteConnection conn;
        private string errmsg;
        SortedDictionary<string, string> dictSta;

        public ModelDB(string _sdbFile)
        {
            this.sdbFile = _sdbFile;
            if (!OpenDataBase()) return;
            dictSta = new SortedDictionary<string, string>();
            dictSta = ReadStationsTable();
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
                MessageBox.Show("Error opening database " + sdbFile + Environment.NewLine + ex.Message);
                return false;
            }
        }
        public void CloseDataBase()
        {
            conn.Close();
            conn = null;
        }
        public SortedDictionary<string, string> ReadStationsTable()
        {
            DataTable dbSta = new DataTable();
            SortedDictionary<string, string> dictSta = new SortedDictionary<string, string>();

            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DISTINCT STATION_ID, STATION_NAME FROM ");
                qry.Append("STATIONS");
                qry.Append(" ORDER BY STATION_ID ");

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(qry.ToString(), conn);
                adapter.Fill(dbSta);
                qry = null;
                adapter = null;

                foreach (DataRow dr in dbSta.Rows)
                {
                    string sta = dr["STATION_ID"].ToString();
                    if (!dictSta.ContainsKey(sta))
                        dictSta.Add(sta, dr["STATION_NAME"].ToString());
                }
                dbSta = null;

                //debug
                foreach (var kv in dictSta)
                    Debug.WriteLine("{0},{1}", kv.Key, kv.Value);
                return dictSta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool FindStation(SortedDictionary<string, string> siteDictionary,
                string site, string siteName)
        {
            //check if dictionary contains station, add if not found
            if (!siteDictionary.ContainsKey(site))
            {
                siteDictionary.Add(site, siteName);

                //insert to database
                StringBuilder qry = new StringBuilder();
                qry.Append("INSERT INTO ");
                qry.Append("[STATIONS] ");
                qry.Append("(STATION_ID, STATION_NAME ");
                qry.Append("VALUES (");
                qry.Append(site + ",");
                qry.Append(siteName + ")");

                SQLiteCommand sqCommand = new SQLiteCommand(qry.ToString());
                sqCommand.Connection = conn;
                conn.Open();
                try
                {
                    sqCommand.ExecuteNonQuery();
                }
                finally
                {
                    conn.Close();
                }
                return true;
            }
            else
                return false; //already contains station
        }

        public int InsertRecordsInModelTable(string StaID, string svar, string Param,
                                        double value, int tstep)
        {
            int nrecs = 0;
            try
            {
                var cmd = new SQLiteCommand(conn);

                StringBuilder qry = new StringBuilder();
                qry.Append("INSERT INTO Model");
                qry.Append("(Station_ID, MetVar, Parameter, Result, Interval)");
                qry.Append(" VALUES(");
                //qry.Append(_rowid_ + ",");
                qry.Append("'" + StaID + "',");
                qry.Append("'" + svar + "',");
                qry.Append("'" + Param + "',");
                qry.Append(value + ",");
                qry.Append(tstep + ")");

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
                return 0;
            }
            return nrecs;
        }
        public int InsertRecordsInStationTable(string StaID, string StaName, string dset, float lat,
                                float lon, float elev, string huc, string state)
        {
            if (dictSta.ContainsKey(StaID)) return 0;
            else
                dictSta.Add(StaID, StaName);

            int nrecs = 0;
            try
            {
                var cmd = new SQLiteCommand(conn);

                StringBuilder qry = new StringBuilder();
                qry.Append("INSERT INTO Stations");
                qry.Append("(Station_ID, Station_Name, Dataset, Latitude," +
                           "Longitude, Elevation, HUC, State)");
                qry.Append(" VALUES(");
                qry.Append("'" + StaID + "',");
                qry.Append("'" + StaName + "',");
                qry.Append("'" + dset + "',");
                qry.Append("'" + lat + "',");
                qry.Append("'" + lon + "',");
                qry.Append("'" + elev + "',");
                qry.Append("'" + huc + "',");
                qry.Append("'" + state + "')");

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
                return 0;

            }
            return nrecs;
        }
        public void ReadVariablesTable()
        {

        }
        public void ReadModelTable()
        {

        }
    }
}
