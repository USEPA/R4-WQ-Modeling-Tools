using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Projections;
using DotSpatial.Symbology;
using DotSpatial.Topology;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;


namespace NCEIData
{
    public partial class frmMet : Form
    {
        private frmMain fMain;
        private int nStation = 0, numState = 0;

        private PolygonLayer selStLayer;
        private List<IFeature> lstFeature;
        private List<string> lstState = new List<string>();
        private List<MetGages> lstGages = new List<MetGages>();
        private int lDatasource;
        private Extent selExtent = new Extent();
        public enum DataSource { GHCN, ISD, HRAIN };
        public List<string> lstSource = new List<string>() { "GHCN", "ISD", "HLYRAIN" };
        private DrawRectangle aoi;
        private double Xmin, Xmax, Ymin, Ymax;
        private List<double> AOICoords = new List<double>();

        public frmMet(frmMain mainform)
        {
            InitializeComponent();
            this.fMain = mainform;
            lDatasource = mainform.optDataSource;
            aoi = fMain.aoi;
        }

        public int GetSitesForAOI()
        {
            fMain.WriteLogFile("Entering GetSitesForAOI ...");

            Cursor.Current = Cursors.WaitCursor;

            //init list
            fMain.dtSites.Rows.Clear();
            if (lstGages.Count > 0) lstGages.Clear();
            {
                //string st = "+-87.3378278256028%2C35.4546432222242%2C-85.99101791242%2C36.3675077871566";
                string st = "+" + AOICoords[0].ToString() + "%2C" +
                    AOICoords[1].ToString() + "%2C" + AOICoords[2].ToString() + "%2C" +
                    AOICoords[3].ToString();

                StringBuilder qrys = new StringBuilder();

                switch (lDatasource)
                {
                    case (int)DataSource.ISD:
                        qrys.Append("https://gis.ncdc.noaa.gov/arcgis/rest/services/cdo/stations/MapServer/7/query?");
                        qrys.Append("where=");
                        //qrys.Append(st);
                        //qrys.Append("%27");
                        qrys.Append("&text=&objectIds=&time=");
                        qrys.Append("&geometry=" + st);
                        qrys.Append("&geometryType=esriGeometryEnvelope&inSR=");
                        qrys.Append("&spatialRel=esriSpatialRelIntersects&relationParam=");
                        qrys.Append("&outFields=STATION%2CAWS%2CWBAN%2CBEG_DATE%2CEND_DATE%2CLATITUDE%2CLONGITUDE%2CELEVATION%2CSTATE");
                        qrys.Append("&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=");
                        qrys.Append("&outSR=&having=&returnIdsOnly=false&returnCountOnly=false&orderByFields=");
                        qrys.Append("&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false");
                        qrys.Append("&gdbVersion=&historicMoment=&returnDistinctValues=false&resultOffset=");
                        qrys.Append("&resultRecordCount=&queryByDistance=&returnExtentOnly=false");
                        qrys.Append("&datumTransformation=&parameterValues=&rangeValues=&quantizationParameters=");
                        qrys.Append("&featureEncoding=esriDefault&f=pjson");
                        break;
                    case (int)DataSource.GHCN:
                        qrys.Append("https://gis.ncdc.noaa.gov/arcgis/rest/services/cdo/stations/MapServer/1/query?");
                        qrys.Append("where=");
                        //qrys.Append(st);
                        //qrys.Append("%27");
                        qrys.Append("&text=&objectIds=&time=");
                        qrys.Append("&geometry=" + st);
                        qrys.Append("&geometryType=esriGeometryEnvelope&inSR=");
                        qrys.Append("&spatialRel=esriSpatialRelIntersects&relationParam=");
                        qrys.Append("&outFields=STATION_ID%2CSTATION_NAME%2CDATA_BEGIN_DATE%2CDATA_END_DATE%2CLONGITUDE%2C");
                        qrys.Append("LATITUDE %2CELEVATION%2CSTATE");
                        qrys.Append("&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=");
                        qrys.Append("&outSR=&having=&returnIdsOnly=false&returnCountOnly=false&orderByFields=");
                        qrys.Append("&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false");
                        qrys.Append("&gdbVersion=&historicMoment=&returnDistinctValues=false&resultOffset=");
                        qrys.Append("&resultRecordCount=&queryByDistance=&returnExtentOnly=false");
                        qrys.Append("&datumTransformation=&parameterValues=&rangeValues=&quantizationParameters=");
                        qrys.Append("&featureEncoding=esriDefault&f=pjson");
                        break;
                    case (int)DataSource.HRAIN:
                        qrys.Append("https://gis.ncdc.noaa.gov/arcgis/rest/services/cdo/stations/MapServer/4/query?");
                        qrys.Append("where=");
                        //qrys.Append(st);
                        //qrys.Append("%27");
                        qrys.Append("&text=&objectIds=&time=");
                        qrys.Append("&geometry=" + st);
                        qrys.Append("&geometryType=esriGeometryEnvelope&inSR=");
                        qrys.Append("&spatialRel=esriSpatialRelIntersects&relationParam=");
                        qrys.Append("&outFields=STATION_ID%2CSTATION_NAME%2CDATA_BEGIN_DATE%2CDATA_END_DATE%2C");
                        qrys.Append("LATITUDE%2CLONGITUDE%2CELEVATION%2CSTATE");
                        qrys.Append("&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=");
                        qrys.Append("&outSR=&having=&returnIdsOnly=false&returnCountOnly=false&orderByFields=");
                        qrys.Append("&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false");
                        qrys.Append("&gdbVersion=&historicMoment=&returnDistinctValues=false&resultOffset=");
                        qrys.Append("&resultRecordCount=&queryByDistance=&returnExtentOnly=false");
                        qrys.Append("&datumTransformation=&parameterValues=&rangeValues=&quantizationParameters=");
                        qrys.Append("&featureEncoding=esriDefault&f=pjson");
                        break;
                }

                string qrystr = qrys.ToString().Trim();
                fMain.WriteLogFile("Query: " + qrystr);

                string response = string.Empty;
                JObject mainObj;
                try
                {
                    response = Download(qrystr);
                    //Debug.WriteLine("Response=" + response);
                    mainObj = JObject.Parse(response);

                    //check for error message in outputObj
                    if (mainObj.Type == Newtonsoft.Json.Linq.JTokenType.Null)
                    {
                        JToken statusObj = mainObj["status"];
                        string statusMessage = statusObj["status_message"].ToString();
                        throw new ArgumentException(statusMessage);
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Cannot execute station query!";
                    ShowError(msg, ex);
                    return -1;
                }

                // TODO: catch when array is empty
                List<string> lstStation = new List<string>();
                List<string> lstStationID = new List<string>();
                List<string> lstWBAN = new List<string>();
                List<string> lstAWS = new List<string>();
                List<string> lstBegDate = new List<string>();
                List<string> lstEndDate = new List<string>();
                List<string> lstSiteLat = new List<string>();
                List<string> lstSiteLon = new List<string>();
                List<string> lstSiteElev = new List<string>();
                List<string> lstState = new List<string>();

                switch (lDatasource)
                {
                    case (int)DataSource.ISD:
                        lstAWS = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.AWS")).ToList();
                        lstWBAN = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.WBAN")).ToList();
                        lstBegDate = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.BEG_DATE")).ToList();
                        lstEndDate = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.END_DATE")).ToList();
                        lstStation = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.STATION")).ToList();
                        break;
                    case (int)DataSource.GHCN:
                        lstStationID = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.STATION_ID")).ToList();
                        lstStation = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.STATION_NAME")).ToList();
                        lstBegDate = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.DATA_BEGIN_DATE")).ToList();
                        lstEndDate = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.DATA_END_DATE")).ToList();
                        break;
                    case (int)DataSource.HRAIN:
                        lstStationID = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.STATION_ID")).ToList();
                        lstStation = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.STATION_NAME")).ToList();
                        lstBegDate = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.DATA_BEGIN_DATE")).ToList();
                        lstEndDate = mainObj["features"].Select(m =>
                            (string)m.SelectToken("attributes.DATA_END_DATE")).ToList();
                        break;
                }

                lstState = mainObj["features"].Select(m =>
                    (string)m.SelectToken("attributes.STATE")).ToList();
                lstSiteLon = mainObj["features"].Select(m =>
                  (string)m.SelectToken("attributes.LONGITUDE")).ToList();
                lstSiteLat = mainObj["features"].Select(m =>
                   (string)m.SelectToken("attributes.LATITUDE")).ToList();
                //lstSiteLon = mainObj["features"].Select(m =>
                //  (string)m.SelectToken("geometry.x")).ToList();
                //lstSiteLat = mainObj["features"].Select(m =>
                //   (string)m.SelectToken("geometry.y")).ToList();
                lstSiteElev = mainObj["features"].Select(m =>
                   (string)m.SelectToken("attributes.ELEVATION")).ToList();

                int nper = 0;
                for (int i = 0; i < lstStation.Count; i++)
                {
                    nper += 1;
                    try
                    {
                        MetGages station = new MetGages();
                        string idstation = string.Empty;
                        string ldate = string.Empty;
                        switch (lDatasource)
                        {
                            case (int)DataSource.ISD:
                                string sta = lstWBAN.ElementAt(i).ToString();
                                string aws = lstAWS.ElementAt(i).ToString();
                                idstation = aws + sta;
                                station.Station_ID = idstation;
                                //station.BEG_DATE = lstBegDate.ElementAt(i).ToString();
                                //station.END_DATE = lstEndDate.ElementAt(i).ToString();
                                station.BEG_DATE = ISDdateToDateTime(lstBegDate.ElementAt(i).ToString());
                                station.END_DATE = ISDdateToDateTime(lstEndDate.ElementAt(i).ToString());
                                //int yr = Convert.ToInt32(station.BEG_DATE.ToString().Substring(0, 4));
                                //station.BegYear = station.BEG_DATE.Year;
                                //yr = Convert.ToInt32(station.END_DATE.ToString().Substring(0, 4));
                                //station.EndYear = station.END_DATE.Year;
                                break;
                            case (int)DataSource.GHCN:
                                station.Station_ID = lstStationID.ElementAt(i).ToString();
                                ldate = lstBegDate.ElementAt(i).ToString();
                                station.BEG_DATE = FromDateTimeOffset(Convert.ToInt64(ldate));//.ToString();
                                ldate = lstEndDate.ElementAt(i).ToString();
                                station.END_DATE = FromDateTimeOffset(Convert.ToInt64(ldate));//.ToString();
                                ldate = null;
                                break;
                            case (int)DataSource.HRAIN:
                                station.Station_ID = lstStationID.ElementAt(i).ToString();
                                ldate = lstBegDate.ElementAt(i).ToString();
                                station.BEG_DATE = FromDateTimeOffset(Convert.ToInt64(ldate));//.ToString();
                                ldate = lstEndDate.ElementAt(i).ToString();
                                station.END_DATE = FromDateTimeOffset(Convert.ToInt64(ldate));//.ToString();
                                ldate = null;
                                break;
                        }

                        station.Station = lstStation.ElementAt(i).ToString();
                        station.STATE = lstState.ElementAt(i).ToString();
                        station.LATITUDE = lstSiteLat.ElementAt(i).ToString();
                        station.LONGITUDE = lstSiteLon.ElementAt(i).ToString();
                        station.ELEVATION = lstSiteElev.ElementAt(i).ToString();

                        fMain.appManager.ProgressHandler.Progress("", nper * 100 / lstStation.Count(), "Querying station: " +
                            idstation + " (" + nper + " of " + lstStation.Count() + ") ...");
                        lstGages.Add(station);

                        DataRow dr = fMain.dtSites.NewRow();
                        dr["Station"] = station.Station;
                        dr["Station_ID"] = station.Station_ID;
                        dr["LATITUDE"] = station.LATITUDE;
                        dr["LONGITUDE"] = station.LONGITUDE;
                        dr["ELEVATION"] = station.ELEVATION;
                        dr["STATE"] = station.STATE;
                        dr["BEG_DATE"] = station.BEG_DATE;
                        dr["END_DATE"] = station.END_DATE;
                        //dr["BegYear"] = station.BegYear;
                        //dr["EndYear"] = station.EndYear;
                        fMain.dtSites.Rows.Add(dr);
                        dr = null;
                        station = null;
                    }
                    catch (Exception ex) { }
                }

                //reset
                lstStation = null;
                lstStationID = null;
                lstAWS = null;
                lstWBAN = null;
                lstBegDate = null;
                lstEndDate = null;
                lstSiteLat = null;
                lstSiteLon = null;
                lstSiteElev = null;
                lstState = null;
            }
            Debug.WriteLine("Count of Gages =" + lstGages.Count.ToString());
            Cursor.Current = Cursors.Default;

            return (lstGages.Count);
        }
        private DateTime ISDdateToDateTime(string isdDate)
        {
            int yr = Convert.ToInt32(isdDate.Substring(0, 4));
            int mo = Convert.ToInt32(isdDate.Substring(4, 2));
            int dy = Convert.ToInt32(isdDate.Substring(6, 2));

            DateTime dt = new DateTime(yr, mo, dy).Date;
            return (dt);
        }
        private string Download(string uri_address)
        {
            Cursor.Current = Cursors.WaitCursor;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            fMain.WriteLogFile("Downloading table of stations ...");

            string reply = string.Empty;
            try
            {
                WebClient wc = new WebClient();
                reply = wc.DownloadString(uri_address);
                if (!wc.IsBusy)
                    wc.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in station service!\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace);
                reply = null;
            }
            Cursor.Current = Cursors.Default;
            return (reply);
        }
        private bool GetAreaOfInterest()
        {
            try
            {
                Extent ext = aoi.RectangleExtent;
                selExtent = ext;

                Xmin = ext.MinX;
                Ymin = ext.MinY;
                Xmax = ext.MaxX;
                Ymax = ext.MaxY;

                aoi.GetGeoCoordinates(Xmin, Ymin, Xmax, Ymax);
                AOICoords = aoi.AOIExtent();
                aoi.Deactivate();

                Debug.WriteLine("XMinLon = " + AOICoords[0].ToString());
                Debug.WriteLine("YMinLat = " + AOICoords[1].ToString());
                Debug.WriteLine("XMaxLon = " + AOICoords[2].ToString());
                Debug.WriteLine("YMaxLat = " + AOICoords[3].ToString());

                if (AOICoords.Count > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                ShowError("Error getting Area of Interest!", ex);
                return false;
            }
        }
        private void ShowError(string msg, Exception ex)
        {
            msg += "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void DrawGagesLayer()
        {
            fMain.WriteLogFile("Entering Draw Gages Layer ...");

            string fname = System.IO.Path.GetTempFileName().Replace(".tmp", ".shp");
            switch (lDatasource)
            {
                case (int)DataSource.ISD:
                    fname = fMain.dataDir + "\\ISD_Gages.shp";
                    break;
                case (int)DataSource.GHCN:
                    fname = fMain.dataDir + "\\GHCN_Gages.shp";
                    break;
                case (int)DataSource.HRAIN:
                    fname = fMain.dataDir + "\\Hourly_RainGages.shp";
                    break;
            }
            Debug.WriteLine("In DrawGagesLayer file = " + fname);

            FeatureSet fs = new FeatureSet(FeatureType.Point);
            // Add Some Columns
            fs.DataTable.Columns.Add(new DataColumn("Station_ID", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Station", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("BegDate", typeof(DateTime)));
            fs.DataTable.Columns.Add(new DataColumn("EndDate", typeof(DateTime)));
            fs.DataTable.Columns.Add(new DataColumn("Lat", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Lon", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Elev", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("State", typeof(string)));

            fs.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;
            //fs.Projection = KnownCoordinateSystems.Projected.World.WebMercator;

            double x, y;
            foreach (var site in lstGages)
            {
                x = Convert.ToDouble(site.LONGITUDE);
                y = Convert.ToDouble(site.LATITUDE);

                Coordinate coord = new DotSpatial.Topology.Coordinate(x, y);
                DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(coord);
                IFeature curFeature = fs.AddFeature(p);
                curFeature.DataRow.BeginEdit();
                curFeature.DataRow["Station"] = site.Station;
                curFeature.DataRow["Station_ID"] = site.Station_ID;
                curFeature.DataRow["BegDate"] = site.BEG_DATE;
                curFeature.DataRow["EndDate"] = site.END_DATE;
                curFeature.DataRow["Lat"] = site.LATITUDE;
                curFeature.DataRow["Lon"] = site.LONGITUDE;
                curFeature.DataRow["Elev"] = site.ELEVATION;
                curFeature.DataRow["State"] = site.STATE;
                curFeature.DataRow.EndEdit();
            }
            //reproject to webmercator
            fs.Reproject(KnownCoordinateSystems.Projected.World.WebMercator);
            fs.SaveAs(fname, true);

            //add to map
            fs = (FeatureSet)FeatureSet.Open(fname);

            MapPointLayer METsites = (MapPointLayer)fMain.appManager.Map.Layers.Add(fs);
            METsites.Symbolizer = new PointSymbolizer(Color.DarkOrange, DotSpatial.Symbology.PointShape.Ellipse, 8);
            METsites.SelectionSymbolizer = new PointSymbolizer(Color.Aqua, DotSpatial.Symbology.PointShape.Ellipse, 8);
            METsites.Symbolizer.SetOutline(Color.Black, 1);
            METsites.DataSet.Name = "Stations";
            METsites.SelectionSymbolizer.SetOutline(Color.Black, 1);

            switch (lDatasource)
            {
                case (int)DataSource.ISD:
                    METsites.LegendText = "ISD Hourly Stations";
                    break;
                case (int)DataSource.GHCN:
                    METsites.LegendText = "GHCN Stations";
                    break;
                case (int)DataSource.HRAIN:
                    METsites.LegendText = "Hourly Rain Stations";
                    break;
            }

            //generate label for facilities
            //string fontname = "Tahoma";
            //double fsize = 8.0;
            //Color fcolor = Color.Black;

            //deprecated method
            //fMain.appManager.Map.AddLabels(METsites, "[" + "WBAN" + "]", new Font("" + fontname + "", (float)fsize), fcolor);

            //selHUCLayer.IsSelected = false;
            METsites.IsSelected = true;

            //fMain.appManager.Map.ViewExtents = fs.Extent; //METsites.DataSet.Extent;
            //set extent to selected state
            fMain.appManager.Map.ViewExtents = selExtent;
            fMain.appManager.Map.Refresh();
            //fMain.dtSites = fs.DataTable;
            fs = null;
        }
        private DateTime FromDateTimeOffset(long aTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(aTime);
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            fMain.appManager.ProgressHandler.Progress("", 0, "");
            fMain.appManager.ProgressHandler.Progress("", 0, "Calling NCEI Service ....");

            selExtent = fMain.appManager.Map.Extent;

            if (GetAreaOfInterest())
            {
                nStation = GetSitesForAOI();
                fMain.WriteLogFile("Number of Stations = " + nStation.ToString());
                //if (nStation > 0)
                //fMain.ShowSitesTable();
            }
            if (nStation > 0)
                DrawGagesLayer();

            this.Close();
            this.Dispose();
        }
        private void frmMet_Load(object sender, EventArgs e)
        {
            //System.Drawing.Point Location = fMain.tabCtrlMain.Location;
            //this.StartPosition = FormStartPosition.Manual;
            //this.Location = Location;
        }
        private void frmMet_FormClosing(object sender, FormClosingEventArgs e)
        {
            fMain.numStations = nStation;
            if (nStation == 0)
                aoi.Deactivate();
        }
        private void frmMet_Deactivate(object sender, EventArgs e)
        {
            fMain.numStations = nStation;
        }
        private void frmMet_Activated(object sender, EventArgs e)
        {
            //System.Drawing.Point Location = fMain.tabCtrlMain.Location;
            //this.StartPosition = FormStartPosition.Manual;
            //this.Location = Location;
        }

    }
}



