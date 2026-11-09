using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WeaModelSDB
{
    public class WeaModelDB
    {
        private string sdbFile;
        private SqliteConnection conn;
        private string errmsg;
        private SortedDictionary<string, string> dictSta;
        private StreamWriter wrlog;
        public WeaModelDB(StreamWriter _wrlog, string _sdbFile)
        {
            this.wrlog = _wrlog;
            this.sdbFile = _sdbFile;
            if (!OpenDataBase()) return;
            dictSta = new SortedDictionary<string, string>();
            dictSta = ReadStationsTable(out errmsg);
        }
        private bool OpenDataBase()
        {
            try
            {
                string connStr = "Data Source=" + sdbFile;
                conn = new SqliteConnection(connStr);
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
        public SortedDictionary<string, string> ReadStationsTable(out string errmsg)
        {
            SortedDictionary<string, string> dictSta = new SortedDictionary<string, string>();
            errmsg = string.Empty;

            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT DISTINCT STATION_ID, STATION_NAME FROM ");
                qry.Append("STATIONS");
                qry.Append(" ORDER BY STATION_ID ");

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string sta = reader["STATION_ID"].ToString();
                        if (!dictSta.ContainsKey(sta))
                            dictSta.Add(sta, reader["STATION_NAME"].ToString());
                    }
                }

                return dictSta;
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
                return null;
            }
        }
        public DataTable GetStationVariables()
        {
            //inner join query from model and stations table
            //SELECT STATION_ID, METVAR, STATION_NAME FROM model INNER JOIN stations
            //ON STATION_ID = STATIONS.STATION_ID;
            DataTable dbSta = new DataTable();

            try
            {
                StringBuilder qry = new StringBuilder();
                //inner join query to get station info
                qry.Append("SELECT DISTINCT MODEL.STATION_ID, MODEL.METVAR, STATIONS.STATION_NAME FROM ");
                qry.Append("MODEL INNER JOIN STATIONS ");
                qry.Append("ON MODEL.STATION_ID = STATIONS.STATION_ID ");
                qry.Append("ORDER BY MODEL.STATION_ID ");

                // Create DataTable structure
                dbSta.Columns.Add("STATION_ID", typeof(string));
                dbSta.Columns.Add("METVAR", typeof(string));
                dbSta.Columns.Add("STATION_NAME", typeof(string));

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DataRow row = dbSta.NewRow();
                        row["STATION_ID"] = reader["STATION_ID"];
                        row["METVAR"] = reader["METVAR"];
                        row["STATION_NAME"] = reader["STATION_NAME"];
                        dbSta.Rows.Add(row);
                    }
                }

                return dbSta;
            }
            catch (Exception ex)
            {
                errmsg = "Error getting station-variable records from Model table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        public int InsertRecordsInModelTable(string StaID, string svar, string Param,
                                        double value, int tstep)
        {
            int nrecs = 0;
            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("INSERT OR REPLACE INTO Model");
                qry.Append("(Station_ID, MetVar, Parameter, Result, Interval)");
                qry.Append(" VALUES(");
                qry.Append("'" + StaID + "',");
                qry.Append("'" + svar + "',");
                qry.Append("'" + Param + "',");
                qry.Append(value + ",");
                qry.Append(tstep + ")");

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error inserting record in Model table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogFile(errmsg);
                return 0;
            }
            return nrecs;
        }
        public int DeleteRecordsFromModelTable(string StaID, string svar, int tstep)
        {
            int nrecs = 0;
            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("DELETE FROM Model ");
                qry.Append("WHERE ");
                qry.Append("STATION_ID = '" + StaID + "' AND ");
                qry.Append("METVAR = '" + svar + "'");

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error inserting record in Model table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogFile(errmsg);
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
                StringBuilder qry = new StringBuilder();
                qry.Append("INSERT OR REPLACE INTO Stations");
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

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error inserting record in Stations table!" + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogFile(errmsg);
                return 0;

            }
            return nrecs;
        }

        /// <summary>
        /// ReadModelParameters 
        /// Reads model parameters from model database
        /// </summary>
        /// <param name="site"></param>
        /// <param name="svar"></param>
        /// <returns></returns>
        public SortedDictionary<string, double> ReadModelParameters(string site, string svar)
        {
            SortedDictionary<string, double> dictModel = new SortedDictionary<string, double>();

            try
            {
                StringBuilder qry = new StringBuilder();
                qry.Append("SELECT STATION_ID, METVAR, PARAMETER, RESULT FROM ");
                qry.Append("MODEL WHERE METVAR = '" + svar + "' ");
                qry.Append("AND STATION_ID = '" + site + "' ");
                qry.Append("ORDER BY PARAMETER ");

                using (var cmd = new SqliteCommand(qry.ToString(), conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string param = reader["PARAMETER"].ToString();
                        float fvalue = Convert.ToSingle(reader["RESULT"]);
                        dictModel.Add(param, fvalue);
                    }
                }
            }
            catch (Exception ex)
            {
                errmsg = "Error getting stochastic model parameters for " + site + ":" + svar + Environment.NewLine + ex.Message +
                    Environment.NewLine + ex.StackTrace;
                MessageBox.Show(errmsg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogFile(errmsg);
                return null;
            }
            return dictModel;
        }
        public void WriteLogFile(string msg)
        {
            wrlog.WriteLine(msg);
            wrlog.AutoFlush = true;
            wrlog.Flush();
        }
    }
}
