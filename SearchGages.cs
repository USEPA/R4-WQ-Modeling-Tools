#define debug
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;


namespace NCEIData
{
    class SearchGages
    {
        private frmMain fMain;
        private int nStation = 0;

        //private PolygonLayer selStLayer;
        //private List<IFeature> lstFeature;
        private List<string> lstState = new List<string>();
        private List<MetGages> lstGages = new List<MetGages>();
        private int lDatasource;
        private Extent selExtent = new Extent();
        public enum DataSource { NLDAS, ISD, HRAIN, GHCN, GLDAS, TRMM, PRISM, CMIP6, EDDE };
        private DrawRectangle aoi;
        private List<double> AOICoords = new List<double>();
        private SortedDictionary<string, string> dictGages;
        private double Xmin, Ymin, Xmax, Ymax;
        private SortedDictionary<string, GridPoint> dictNLDAS, dictEDDE;
        private SortedDictionary<string, List<float>> dictEDDEgrid;
        private SortedDictionary<string, List<float>> dictPRISMgrid;

        public SearchGages(frmMain _fmain)
        {
            this.fMain = _fmain;
            lDatasource = _fmain.optDataSource;
            aoi = _fmain.aoi;
            this.dictGages = fMain.dictGages;
            this.dictEDDEgrid = fMain.dictEDDEgrids;
            this.dictPRISMgrid = fMain.dictPRISMgrids;
        }
        private int GetGridForAOI_NLDAS()
        {
            Cursor.Current = Cursors.WaitCursor;
            // XminLon= AOICoords[0] NW lon, YminLat= AOICoords[1] SE lat
            // XmaxLon= AOICoords[2] SE lon, YmaxLat= AOICoords[3] NW lat
            double xxmin, yymin, xxmax, yymax;
            int ngrid = 0, ifac = 0; int xgrid = 0, ygrid = 0;
            int latSE, lonSE, latNW, lonNW;
            double gridint = 0.125, halfgrid = 0.0625;

            try
            {
                //NW grid
                ifac = (int)Math.Floor((AOICoords[0] - Math.Floor(AOICoords[0])) / 0.125);
                xxmin = Math.Floor(AOICoords[0]) + ifac * 0.125;
                ifac = (int)Math.Ceiling((AOICoords[3] - Math.Ceiling(AOICoords[3])) / 0.125);
                yymax = Math.Ceiling(AOICoords[3]) + ifac * 0.125;

                //SE grid
                ifac = (int)Math.Ceiling((AOICoords[2] - Math.Ceiling(AOICoords[2])) / 0.125);
                xxmax = Math.Ceiling(AOICoords[2]) + ifac * 0.125;
                ifac = (int)Math.Floor((AOICoords[1] - Math.Floor(AOICoords[1])) / 0.125);
                yymin = Math.Floor(AOICoords[1]) + ifac * 0.125;

                //numgrids x and y
                xgrid = Convert.ToInt32(Math.Abs((xxmax - xxmin) / 0.125));
                ygrid = Convert.ToInt32(Math.Abs((yymax - yymin) / 0.125));

                Debug.WriteLine("Xmin={0},Ymax={1},Xmax={2},Ymin={3}", xxmin.ToString("F3"),
                    yymax.ToString("F3"), xxmax.ToString("F3"), yymin.ToString("F3"));
                Debug.WriteLine("xgri={0},ygrid={1}", xgrid.ToString(), ygrid.ToString());

                //get grid indices for box
                lonSE = GetIntGridCoord("lonSE", xxmax, 0, 464, -125, -67);
                lonNW = GetIntGridCoord("lonNW", xxmin, 0, 464, -125, -67);
                latSE = GetIntGridCoord("latSE", yymin, 0, 224, 25, 53);
                latNW = GetIntGridCoord("latNW", yymax, 0, 224, 25, 53);

                int icol = lonNW - 1;
                double xpt = 0.0, ypt = 0.0;
                dictNLDAS = new SortedDictionary<string, GridPoint>();
                for (double x = xxmin; x <= xxmax; x += gridint)
                {
                    icol++;
                    xpt = x + halfgrid;
                    int irow = latSE - 1;
                    string scol = "X" + icol.ToString("000");
                    for (double y = yymin; y <= yymax; y += gridint)
                    {
                        irow++;
                        ypt = y + halfgrid;
                        //Debug.WriteLine("{0},{1},{2},{3}",
                        //    icol.ToString("000"), irow.ToString("000"), xpt.ToString("F4"), ypt.ToString("F4"));
                        string srow = "Y" + irow.ToString("000");
                        GridPoint pt = new GridPoint();
                        pt.xlon = xpt;
                        pt.ylat = ypt;
                        if (!dictNLDAS.ContainsKey(scol + srow))
                            dictNLDAS.Add(scol + srow, pt);
                        pt = null;
                    }
                }
                ngrid = dictNLDAS.Keys.Count;
                //debug
                Debug.WriteLine("Num Grids = " + ngrid.ToString());
                //foreach (KeyValuePair<string,GridPoint>kv in dictNLDAS)
                //{
                //    string ky = kv.Key;
                //    GridPoint pt = kv.Value;
                //    Debug.WriteLine("{0},{1},{2}", ky, pt.xlon.ToString("F4"), pt.ylat.ToString("F4"));
                //}
            }
            catch (Exception ex)
            {
                string msg = "Error getting NLDAS grid indices ...";
                MessageBox.Show(msg + "\r\n" + ex.Message + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ngrid = 0;
            }
            Cursor.Current = Cursors.Default;
            return ngrid;
        }
        private int GetGridForAOI_GLDAS()
        {
            Cursor.Current = Cursors.WaitCursor;
            // XminLon= AOICoords[0] NW lon, YminLat= AOICoords[1] SE lat
            // XmaxLon= AOICoords[2] SE lon, YmaxLat= AOICoords[3] NW lat
            double xxmin, yymin, xxmax, yymax;
            int ngrid = 0, ifac = 0; int xgrid = 0, ygrid = 0;
            int latSE, lonSE, latNW, lonNW;
            double gridint = 0.25, halfgrid = 0.125;
            try
            {
                //NW grid
                ifac = (int)Math.Floor((AOICoords[0] - Math.Floor(AOICoords[0])) / 0.25);
                xxmin = Math.Floor(AOICoords[0]) + ifac * 0.25;
                ifac = (int)Math.Ceiling((AOICoords[3] - Math.Ceiling(AOICoords[3])) / 0.25);
                yymax = Math.Ceiling(AOICoords[3]) + ifac * 0.25;

                //SE grid
                ifac = (int)Math.Ceiling((AOICoords[2] - Math.Ceiling(AOICoords[2])) / 0.25);
                xxmax = Math.Ceiling(AOICoords[2]) + ifac * 0.25;
                ifac = (int)Math.Floor((AOICoords[1] - Math.Floor(AOICoords[1])) / 0.25);
                yymin = Math.Floor(AOICoords[1]) + ifac * 0.25;

                //numgrids x and y
                xgrid = Convert.ToInt32(Math.Abs((xxmax - xxmin) / 0.25));
                ygrid = Convert.ToInt32(Math.Abs((yymax - yymin) / 0.25));

                Debug.WriteLine("Xmin={0},Ymax={1},Xmax={2},Ymin={3}", xxmin.ToString("F3"),
                    yymax.ToString("F3"), xxmax.ToString("F3"), yymin.ToString("F3"));
                Debug.WriteLine("xgri={0},ygrid={1}", xgrid.ToString(), ygrid.ToString());

                //get grid indices for box
                lonSE = GetIntGridCoord("lonSE", xxmax, 0, 1440, -180, 180);
                lonNW = GetIntGridCoord("lonNW", xxmin, 0, 1440, -180, 180);
                latSE = GetIntGridCoord("latSE", yymin, 0, 600, -60, 90);
                latNW = GetIntGridCoord("latNW", yymax, 0, 600, -60, 90);

                int icol = lonNW - 1;
                double xpt = 0.0, ypt = 0.0;
                dictNLDAS = new SortedDictionary<string, GridPoint>();
                for (double x = xxmin; x <= xxmax; x += gridint)
                {
                    icol++;
                    xpt = x + halfgrid;
                    int irow = latSE - 1;
                    //string scol = "X" + icol.ToString("0000");
                    string scol = "G" + icol.ToString("0000");
                    for (double y = yymin; y <= yymax; y += gridint)
                    {
                        irow++;
                        ypt = y + halfgrid;
                        //Debug.WriteLine("{0},{1},{2},{3}",
                        //    icol.ToString("000"), irow.ToString("000"), xpt.ToString("F4"), ypt.ToString("F4"));
                        //string srow = "Y" + irow.ToString("000");
                        string srow = irow.ToString("000");
                        GridPoint pt = new GridPoint();
                        pt.xlon = xpt;
                        pt.ylat = ypt;
                        if (!dictNLDAS.ContainsKey(scol + srow))
                            dictNLDAS.Add(scol + srow, pt);
                        pt = null;
                    }
                }
                ngrid = dictNLDAS.Keys.Count;
                //debug
                Debug.WriteLine("Num Grids = " + ngrid.ToString());
                //foreach (KeyValuePair<string,GridPoint>kv in dictNLDAS)
                //{
                //    string ky = kv.Key;
                //    GridPoint pt = kv.Value;
                //    Debug.WriteLine("{0},{1},{2}", ky, pt.xlon.ToString("F4"), pt.ylat.ToString("F4"));
                //}
            }
            catch (Exception ex)
            {
                string msg = "Error getting GLDAS grid indices ...";
                MessageBox.Show(msg + "\r\n" + ex.Message + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ngrid = 0;
            }
            Cursor.Current = Cursors.Default;
            return ngrid;
        }
        private int GetGridForAOI_TRMM()
        {
            Cursor.Current = Cursors.WaitCursor;
            // XminLon= AOICoords[0] NW lon, YminLat= AOICoords[1] SE lat
            // XmaxLon= AOICoords[2] SE lon, YmaxLat= AOICoords[3] NW lat
            double xxmin, yymin, xxmax, yymax;
            int ngrid = 0, ifac = 0; int xgrid = 0, ygrid = 0;
            int latSE, lonSE, latNW, lonNW;
            double gridint = 0.25, halfgrid = 0.125;
            try
            {
                //NW grid
                ifac = (int)Math.Floor((AOICoords[0] - Math.Floor(AOICoords[0])) / 0.25);
                xxmin = Math.Floor(AOICoords[0]) + ifac * 0.25;
                ifac = (int)Math.Ceiling((AOICoords[3] - Math.Ceiling(AOICoords[3])) / 0.25);
                yymax = Math.Ceiling(AOICoords[3]) + ifac * 0.25;

                //SE grid
                ifac = (int)Math.Ceiling((AOICoords[2] - Math.Ceiling(AOICoords[2])) / 0.25);
                xxmax = Math.Ceiling(AOICoords[2]) + ifac * 0.25;
                ifac = (int)Math.Floor((AOICoords[1] - Math.Floor(AOICoords[1])) / 0.25);
                yymin = Math.Floor(AOICoords[1]) + ifac * 0.25;

                //numgrids x and y
                xgrid = Convert.ToInt32(Math.Abs((xxmax - xxmin) / 0.25));
                ygrid = Convert.ToInt32(Math.Abs((yymax - yymin) / 0.25));

                Debug.WriteLine("Xmin={0},Ymax={1},Xmax={2},Ymin={3}", xxmin.ToString("F3"),
                    yymax.ToString("F3"), xxmax.ToString("F3"), yymin.ToString("F3"));
                Debug.WriteLine("xgri={0},ygrid={1}", xgrid.ToString(), ygrid.ToString());

                //get grid indices for box
                lonSE = GetIntGridCoord("lonSE", xxmax, 0, 1440, -180, 180);
                lonNW = GetIntGridCoord("lonNW", xxmin, 0, 1440, -180, 180);
                latSE = GetIntGridCoord("latSE", yymin, 0, 400, -50, 50);
                latNW = GetIntGridCoord("latNW", yymax, 0, 400, -50, 50);

                int icol = lonNW - 1;
                double xpt = 0.0, ypt = 0.0;
                dictNLDAS = new SortedDictionary<string, GridPoint>();
                for (double x = xxmin; x <= xxmax; x += gridint)
                {
                    icol++;
                    xpt = x + halfgrid;
                    int irow = latSE - 1;
                    //string scol = "X" + icol.ToString("000");
                    string scol = "T" + icol.ToString("0000");
                    for (double y = yymin; y <= yymax; y += gridint)
                    {
                        irow++;
                        ypt = y + halfgrid;
                        //Debug.WriteLine("{0},{1},{2},{3}",
                        //    icol.ToString("000"), irow.ToString("000"), xpt.ToString("F4"), ypt.ToString("F4"));
                        //string srow = "Y" + irow.ToString("000");
                        string srow = irow.ToString("000");
                        GridPoint pt = new GridPoint();
                        pt.xlon = xpt;
                        pt.ylat = ypt;
                        if (!dictNLDAS.ContainsKey(scol + srow))
                            dictNLDAS.Add(scol + srow, pt);
                        pt = null;
                    }
                }
                ngrid = dictNLDAS.Keys.Count;
                //debug
                Debug.WriteLine("Num Grids = " + ngrid.ToString());
                //foreach (KeyValuePair<string,GridPoint>kv in dictNLDAS)
                //{
                //    string ky = kv.Key;
                //    GridPoint pt = kv.Value;
                //    Debug.WriteLine("{0},{1},{2}", ky, pt.xlon.ToString("F4"), pt.ylat.ToString("F4"));
                //}
            }
            catch (Exception ex)
            {
                string msg = "Error getting TRMM grid indices ...";
                MessageBox.Show(msg + "\r\n" + ex.Message + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ngrid = 0;
            }
            Cursor.Current = Cursors.Default;
            return ngrid;
        }

        private int GetGridForAOI_CMIP6()
        {
            Cursor.Current = Cursors.WaitCursor;
            // XminLon= AOICoords[0] NW lon, YminLat= AOICoords[1] SE lat
            // XmaxLon= AOICoords[2] SE lon, YmaxLat= AOICoords[3] NW lat
            double xxmin, yymin, xxmax, yymax;
            int ngrid = 0, ifac = 0; int xgrid = 0, ygrid = 0;
            int latSE, lonSE, latNW, lonNW;
            double gridint = 0.25, halfgrid = 0.125;
            try
            {
                //NW grid
                ifac = (int)Math.Floor((AOICoords[0] - Math.Floor(AOICoords[0])) / 0.25);
                xxmin = Math.Floor(AOICoords[0]) + ifac * 0.25;
                ifac = (int)Math.Ceiling((AOICoords[3] - Math.Ceiling(AOICoords[3])) / 0.25);
                yymax = Math.Ceiling(AOICoords[3]) + ifac * 0.25;

                //SE grid
                ifac = (int)Math.Ceiling((AOICoords[2] - Math.Ceiling(AOICoords[2])) / 0.25);
                xxmax = Math.Ceiling(AOICoords[2]) + ifac * 0.25;
                ifac = (int)Math.Floor((AOICoords[1] - Math.Floor(AOICoords[1])) / 0.25);
                yymin = Math.Floor(AOICoords[1]) + ifac * 0.25;

                //numgrids x and y
                xgrid = Convert.ToInt32(Math.Abs((xxmax - xxmin) / 0.25));
                ygrid = Convert.ToInt32(Math.Abs((yymax - yymin) / 0.25));

                //Debug.WriteLine("Xmin={0},Ymax={1},Xmax={2},Ymin={3}", xxmin.ToString("F3"),
                //    yymax.ToString("F3"), xxmax.ToString("F3"), yymin.ToString("F3"));
                //Debug.WriteLine("xgri={0},ygrid={1}", xgrid.ToString(), ygrid.ToString());

                //get grid indices for box
                lonSE = GetIntGridCoord("lonSE", xxmax, 1, 1440, -179.875, 179.875);
                lonNW = GetIntGridCoord("lonNW", xxmin, 1, 1440, -179.875, 179.875);
                latSE = GetIntGridCoord("latSE", yymin, 1, 600, -59.875, 89.875);
                latNW = GetIntGridCoord("latNW", yymax, 1, 600, -59.875, 89.875);

                int icol = lonNW - 1;
                double xpt = 0.0, ypt = 0.0;
                dictNLDAS = new SortedDictionary<string, GridPoint>();
                for (double x = xxmin; x <= xxmax; x += gridint)
                {
                    icol++;
                    xpt = x + halfgrid;
                    int irow = latSE - 1;
                    string scol = "C" + icol.ToString("0000");
                    for (double y = yymin; y <= yymax; y += gridint)
                    {
                        irow++;
                        ypt = y + halfgrid;
                        //Debug.WriteLine("{0},{1},{2},{3}",
                        //    icol.ToString("000"), irow.ToString("000"), xpt.ToString("F4"), ypt.ToString("F4"));
                        //string srow = "Y" + irow.ToString("000");
                        string srow = irow.ToString("000");
                        GridPoint pt = new GridPoint();
                        pt.xlon = xpt;
                        pt.ylat = ypt;
                        if (!dictNLDAS.ContainsKey(scol + srow))
                            dictNLDAS.Add(scol + srow, pt);
                        pt = null;
                    }
                }
                ngrid = dictNLDAS.Keys.Count;
#if debug
                //debug
                Debug.WriteLine("Num Grids = " + ngrid.ToString());
                foreach (KeyValuePair<string,GridPoint>kv in dictNLDAS)
                {
                    string ky = kv.Key;
                    GridPoint pt = kv.Value;
                    Debug.WriteLine("{0},{1},{2}", ky, pt.xlon.ToString("F4"), pt.ylat.ToString("F4"));
                }
#endif             
            }
            catch (Exception ex)
            {
                string msg = "Error getting CMIP6 grid indices ...";
                MessageBox.Show(msg + "\r\n" + ex.Message + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ngrid = 0;
            }
            Cursor.Current = Cursors.Default;
            return ngrid;
        }

        private int GetGridForAOI_EDDE()
        {
            Cursor.Current = Cursors.WaitCursor;
            // XminLon= AOICoords[0] W lon, YminLat= AOICoords[1] S lat
            // XmaxLon= AOICoords[2] E lon, YmaxLat= AOICoords[3] N lat
            double xxmin = AOICoords[0];
            double yymin = AOICoords[1];
            double xxmax = AOICoords[2];
            double yymax = AOICoords[3];

            int ngrid = 0; 
            float lat, lon;
            
            try
            {
                dictNLDAS = new SortedDictionary<string, GridPoint>();

                foreach (KeyValuePair<string, List<float>> kv in dictEDDEgrid)
                {
                    string grid = kv.Key;
                    List<float> latlon = kv.Value;
                    lon = latlon[0]; lat = latlon[1];

                    //brute force check if within AOI 
                    if (lon <= xxmax && lon >= xxmin)
                    {
                        if (lat <= yymax && lat >= yymin)
                        {
                            GridPoint pt = new GridPoint();
                            pt.xlon = Convert.ToDouble(lon);
                            pt.ylat = Convert.ToDouble(lat);
                            if (!dictNLDAS.ContainsKey(grid))
                                dictNLDAS.Add(grid, pt);
                            pt = null;
                        }
                    }
                }
                ngrid = dictNLDAS.Keys.Count;
#if debug
                //debug
                Debug.WriteLine("Num Grids = " + ngrid.ToString());
                foreach (KeyValuePair<string, GridPoint> kv in dictNLDAS)
                {
                    string ky = kv.Key;
                    GridPoint pt = kv.Value;
                    Debug.WriteLine("{0},{1},{2}", ky, pt.xlon.ToString("F4"), pt.ylat.ToString("F4"));
                }
#endif             
            }
            catch (Exception ex)
            {
                string msg = "Error getting EDDE grid indices ...";
                MessageBox.Show(msg + "\r\n" + ex.Message + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ngrid = 0;
            }
            Cursor.Current = Cursors.Default;
            return ngrid;
        }

        private int GetGridForAOI_PRISM()
        {
            Cursor.Current = Cursors.WaitCursor;
            // XminLon= AOICoords[0] W lon, YminLat= AOICoords[1] S lat
            // XmaxLon= AOICoords[2] E lon, YmaxLat= AOICoords[3] N lat
            double xxmin = AOICoords[0];
            double yymin = AOICoords[1];
            double xxmax = AOICoords[2];
            double yymax = AOICoords[3];

            int ngrid = 0;
            float lat, lon;

            try
            {
                dictNLDAS = new SortedDictionary<string, GridPoint>();

                foreach (KeyValuePair<string, List<float>> kv in dictPRISMgrid)
                {
                    string grid = kv.Key;
                    List<float> latlon = kv.Value;
                    lon = latlon[0]; lat = latlon[1];

                    //brute force check if within AOI 
                    if (lon <= xxmax && lon >= xxmin)
                    {
                        if (lat <= yymax && lat >= yymin)
                        {
                            GridPoint pt = new GridPoint();
                            pt.xlon = Convert.ToDouble(lon);
                            pt.ylat = Convert.ToDouble(lat);
                            if (!dictNLDAS.ContainsKey(grid))
                                dictNLDAS.Add(grid, pt);
                            pt = null;
                        }
                    }
                }
                ngrid = dictNLDAS.Keys.Count;
#if debug
                //debug
                Debug.WriteLine("Num Grids = " + ngrid.ToString());
                foreach (KeyValuePair<string, GridPoint> kv in dictNLDAS)
                {
                    string ky = kv.Key;
                    GridPoint pt = kv.Value;
                    Debug.WriteLine("{0},{1},{2}", ky, pt.xlon.ToString("F7"), pt.ylat.ToString("F7"));
                }
#endif             
            }
            catch (Exception ex)
            {
                string msg = "Error getting PRISM grid indices ...";
                MessageBox.Show(msg + "\r\n" + ex.Message + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ngrid = 0;
            }
            Cursor.Current = Cursors.Default;
            return ngrid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="degpt"></param>
        /// <param name="xmn"></param>
        /// <param name="xmx"></param>
        /// <param name="degmn"></param>
        /// <param name="degmx"></param>
        /// <returns></returns>
        private int GetIntGridCoord(string loc, double degpt, int xmn, int xmx, double degmn, double degmx)
        {
            //Debug.WriteLine("Entering GetIntGridCoord...");
            int idx;
            int ixint = xmx - xmn;

            double x = xmn + ixint * (degpt - degmn) / (degmx - degmn);
            idx = Convert.ToInt32(Math.Floor(x));
            Debug.WriteLine("{0}, pt={1}, xindex = {2}, index={3}",
                loc, degpt.ToString(), x.ToString("F4"), idx.ToString());
            return idx;
        }
        private int GetSitesForAOI()
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
                                station.TZONE = "0";
                                // "AWS": "722285", "WBAN": "03896",
                                string sta = lstWBAN.ElementAt(i).ToString();
                                string aws = lstAWS.ElementAt(i).ToString();
                                idstation = aws + sta;
                                station.Station_ID = idstation;
                                station.BEG_DATE = ISDdateToDateTime(lstBegDate.ElementAt(i).ToString());
                                station.END_DATE = ISDdateToDateTime(lstEndDate.ElementAt(i).ToString());
                                station.Station = lstStation.ElementAt(i).ToString();
                                if (!(lstState.ElementAt(i) == null))
                                    station.STATE = lstState.ElementAt(i).ToString();
                                else
                                    station.STATE = string.Empty;

                                station.LATITUDE = lstSiteLat.ElementAt(i).ToString();
                                station.LONGITUDE = lstSiteLon.ElementAt(i).ToString();

                                if (!(lstSiteElev.ElementAt(i) == null))
                                    station.ELEVATION = lstSiteElev.ElementAt(i).ToString();
                                else
                                    station.ELEVATION = string.Empty;
                                if (GageInCoverage(idstation))
                                    lstGages.Add(station);
                                break;
                            case (int)DataSource.GHCN:
                                //"STATION_ID": "GHCND:US1GALR0001"
                                station.TZONE = "0";
                                string ghcn = lstStationID.ElementAt(i).ToString();
                                station.Station_ID = ghcn.Split(':').ElementAt(1);
                                ldate = lstBegDate.ElementAt(i).ToString();
                                station.BEG_DATE = FromDateTimeOffset(Convert.ToInt64(ldate));//.ToString();
                                ldate = lstEndDate.ElementAt(i).ToString();
                                station.END_DATE = FromDateTimeOffset(Convert.ToInt64(ldate));//.ToString();
                                station.Station = lstStation.ElementAt(i).ToString();

                                if (!(lstState.ElementAt(i) == null))
                                    station.STATE = lstState.ElementAt(i).ToString();
                                else
                                    station.STATE = string.Empty;

                                station.LATITUDE = lstSiteLat.ElementAt(i).ToString();
                                station.LONGITUDE = lstSiteLon.ElementAt(i).ToString();

                                if (!(lstSiteElev.ElementAt(i) == null))
                                    station.ELEVATION = lstSiteElev.ElementAt(i).ToString();
                                else
                                    station.ELEVATION = string.Empty;

                                ldate = null;
                                if (GageInCoverage(station.Station_ID))
                                    lstGages.Add(station);
                                break;
                            case (int)DataSource.HRAIN:
                                // "STATION_ID": "COOP:092161"
                                station.TZONE = "0";
                                string rain = lstStationID.ElementAt(i).ToString();
                                station.Station_ID = "USC00" + rain.Split(':').ElementAt(1);
                                ldate = lstBegDate.ElementAt(i).ToString();
                                station.BEG_DATE = FromDateTimeOffset(Convert.ToInt64(ldate));//.ToString();
                                ldate = lstEndDate.ElementAt(i).ToString();
                                station.END_DATE = FromDateTimeOffset(Convert.ToInt64(ldate));//.ToString();
                                station.Station = lstStation.ElementAt(i).ToString();
                                station.STATE = lstState.ElementAt(i).ToString();
                                station.LATITUDE = lstSiteLat.ElementAt(i).ToString();
                                station.LONGITUDE = lstSiteLon.ElementAt(i).ToString();
                                station.ELEVATION = lstSiteElev.ElementAt(i).ToString();
                                ldate = null;
                                if (GageInCoverage(station.Station_ID))
                                    lstGages.Add(station);
                                break;
                        }

                        fMain.appManager.ProgressHandler.Progress("", nper * 100 / lstStation.Count(), "Querying station: " +
                            idstation + " (" + nper + " of " + lstStation.Count() + ") ...");

                        DataRow dr = fMain.dtSites.NewRow();
                        dr["Station"] = station.Station;
                        dr["Station_ID"] = station.Station_ID;
                        dr["LATITUDE"] = station.LATITUDE;
                        dr["LONGITUDE"] = station.LONGITUDE;
                        dr["ELEVATION"] = station.ELEVATION;
                        dr["STATE"] = station.STATE;
                        dr["BEG_DATE"] = station.BEG_DATE;
                        dr["END_DATE"] = station.END_DATE;
                        dr["ZONE"] = station.TZONE;
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
        private bool GageInCoverage(string sta)
        {
            if (dictGages.ContainsKey(sta))
                return true;
            else
                return false;
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
            //fMain.WriteLogFile("Downloading table of stations ...");

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

                Debug.WriteLine("In GetAreaOfInterest :AOI XMinLon (W) = " + AOICoords[0].ToString());
                Debug.WriteLine("In GetAreaOfInterest :AOI YMinLat (S) = " + AOICoords[1].ToString());
                Debug.WriteLine("In GetAreaOfInterest :AOI XMaxLon (E) = " + AOICoords[2].ToString());
                Debug.WriteLine("In GetAreaOfInterest :AOI YMaxLat (N) = " + AOICoords[3].ToString());

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

        private void DrawGagesLayer()
        {
            fMain.WriteLogFile("Entering Draw Gages Layer ...");

            string fname = System.IO.Path.GetTempFileName().Replace(".tmp", ".shp");
            string gisdir = Path.Combine(fMain.cacheDir, "GIS");
            fMain.tzshape = Shapefile.OpenFile(fMain.tzshp);

            switch (lDatasource)
            {
                case (int)DataSource.ISD:
                    fname = Path.Combine(gisdir, "ISD_Gages.shp");
                    break;
                case (int)DataSource.GHCN:
                    fname = Path.Combine(gisdir, "GHCN_Gages.shp");
                    break;
                case (int)DataSource.HRAIN:
                    fname = Path.Combine(gisdir, "Hourly_RainGages.shp");
                    break;
            }
            //Debug.WriteLine("In DrawGagesLayer file = " + fname);

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
            fs.DataTable.Columns.Add(new DataColumn("Zone", typeof(float)));

            fs.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

            double x, y;
            foreach (var site in lstGages)
            {
                x = Convert.ToDouble(site.LONGITUDE);
                y = Convert.ToDouble(site.LATITUDE);

                float tzone = GetTimeZone(x, y);

                Coordinate coord = new DotSpatial.Topology.Coordinate(x, y);
                DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(coord);
                IFeature curFeature = fs.AddFeature(p);
                curFeature.DataRow.BeginEdit();
                string[] stname = site.Station.Split(',');
                //curFeature.DataRow["Station"] = site.Station;
                curFeature.DataRow["Station"] = stname[0];
                curFeature.DataRow["Station_ID"] = site.Station_ID;
                curFeature.DataRow["BegDate"] = site.BEG_DATE;
                curFeature.DataRow["EndDate"] = site.END_DATE;
                curFeature.DataRow["Lat"] = site.LATITUDE;
                curFeature.DataRow["Lon"] = site.LONGITUDE;
                curFeature.DataRow["Elev"] = site.ELEVATION;
                curFeature.DataRow["State"] = site.STATE;
                curFeature.DataRow["Zone"] = tzone;
                curFeature.DataRow.EndEdit();
            }
            //reproject to webmercator
            fs.Reproject(KnownCoordinateSystems.Projected.World.WebMercator);
            fs.SaveAs(fname, true);

            //add to map
            fs = (FeatureSet)FeatureSet.Open(fname);

            MapPointLayer METsites = (MapPointLayer)fMain.appManager.Map.Layers.Add(fs);
            METsites.Symbolizer = new PointSymbolizer(Color.DarkOrange, DotSpatial.Symbology.PointShape.Ellipse, 8);
            METsites.SelectionSymbolizer = new PointSymbolizer(Color.Aqua, DotSpatial.Symbology.PointShape.Hexagon, 12);
            METsites.Symbolizer.SetOutline(Color.Black, 1);
            METsites.DataSet.Name = "Stations";
            METsites.SelectionSymbolizer.SetOutline(Color.Red, 2);

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

            //string slabel = "[Station]";
            MapLabelLayer fslbl = new MapLabelLayer(fs);
            fslbl.Symbology.Categories[0].Expression = "[Station]";
            //if (lDatasource==(int)DataSource.GHCN || lDatasource == (int)DataSource.HRAIN)
            //    fslbl.Symbology.Categories[0].Expression = slabel.Substring(0,slabel.IndexOf(',')-1);
            //else
            //    fslbl.Symbology.Categories[0].Expression = slabel;
            fslbl.Symbolizer.Orientation = ContentAlignment.TopRight;
            fslbl.Symbolizer.PreventCollisions = true;
            fslbl.Symbolizer.LabelPlacementMethod = LabelPlacementMethod.Center;
            METsites.LabelLayer = fslbl;
            METsites.ShowLabels = true;
            METsites.IsSelected = true;

            //fMain.appManager.Map.ViewExtents = fs.Extent; //METsites.DataSet.Extent;
            fMain.appManager.Map.ViewExtents = selExtent;
            fMain.appManager.Map.Refresh();
            //fMain.dtSites = fs.DataTable;
            fs = null;
        }

        private void DrawGridLayer(string source)
        {
            fMain.tzshape = Shapefile.OpenFile(fMain.tzshp);
            fMain.WriteLogFile("Entering Draw " + source + " Grid Layer ...");

            string fname = System.IO.Path.GetTempFileName().Replace(".tmp", ".shp");
            string gisdir = Path.Combine(fMain.cacheDir, "GIS");

            fname = Path.Combine(gisdir, source + "_Grid.shp");

            Debug.WriteLine("In DrawGridLayer file = " + fname);

            FeatureSet fs = new FeatureSet(FeatureType.Point);
            // Add Some Columns
            fs.DataTable.Columns.Add(new DataColumn("Station_ID", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Station", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("BegDate", typeof(DateTime)));
            fs.DataTable.Columns.Add(new DataColumn("EndDate", typeof(DateTime)));
            fs.DataTable.Columns.Add(new DataColumn("Lat", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Lon", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Elev", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("Zone", typeof(float)));

            fs.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

            double x, y;
            foreach (KeyValuePair<string, GridPoint> site in dictNLDAS)
            {
                string idsta = site.Key;
                GridPoint pt = site.Value;

                x = pt.xlon;
                y = pt.ylat;

                float tzone = GetTimeZone(x, y);
                
                Coordinate coord = new DotSpatial.Topology.Coordinate(x, y);
                DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(coord);
                IFeature curFeature = fs.AddFeature(p);
                curFeature.DataRow.BeginEdit();
                curFeature.DataRow["Station"] = idsta;
                curFeature.DataRow["Station_ID"] = idsta;
                curFeature.DataRow["Lat"] = y.ToString("F7");
                curFeature.DataRow["Lon"] = x.ToString("F7");
                curFeature.DataRow["Zone"] = tzone;
                curFeature.DataRow["Elev"] = "2.0";
                curFeature.DataRow.EndEdit();
            }
            //reproject to webmercator
            fs.Reproject(KnownCoordinateSystems.Projected.World.WebMercator);
            fs.SaveAs(fname, true);

            //add to map
            fs = (FeatureSet)FeatureSet.Open(fname);

            MapPointLayer NLDASsites = (MapPointLayer)fMain.appManager.Map.Layers.Add(fs);
            NLDASsites.Symbolizer = new PointSymbolizer(Color.DarkOrange, DotSpatial.Symbology.PointShape.Ellipse, 8);
            NLDASsites.SelectionSymbolizer = new PointSymbolizer(Color.Aqua, DotSpatial.Symbology.PointShape.Hexagon, 12);
            NLDASsites.Symbolizer.SetOutline(Color.Black, 1);
            NLDASsites.SelectionSymbolizer.SetOutline(Color.Red, 2);

            //if (source.Contains("NLDAS"))
            //{
            //    NLDASsites.LegendText = "NLDAS Stations";
            //    NLDASsites.DataSet.Name = "Stations";
            //}
            //else
            //{
            //    NLDASsites.LegendText = "GLDAS Stations";
            //    NLDASsites.DataSet.Name = "Stations";
            //}
            NLDASsites.LegendText = source + " Stations";
            NLDASsites.DataSet.Name = "Stations";

            MapLabelLayer fslbl = new MapLabelLayer(fs);
            fslbl.Symbology.Categories[0].Expression = "[Station_ID]";
            fslbl.Symbolizer.Orientation = ContentAlignment.TopRight;
            fslbl.Symbolizer.PreventCollisions = false;
            fslbl.Symbolizer.LabelPlacementMethod = LabelPlacementMethod.Center;
            NLDASsites.LabelLayer = fslbl;
            NLDASsites.ShowLabels = true;
            NLDASsites.IsSelected = true;

            //fMain.appManager.Map.ViewExtents = fs.Extent; //METsites.DataSet.Extent;
            fMain.appManager.Map.ViewExtents = selExtent;
            fMain.appManager.Map.Refresh();
            //fMain.dtSites = fs.DataTable;
            fs = null;
        }

        private float GetTimeZone(double x, double y)
        {
            int tz = 0;
            Coordinate pt = new Coordinate(x, y);
            DataRow dr; 

            foreach (Feature f in fMain.tzshape.Features)
            {
                Polygon pg = f.BasicGeometry as Polygon;
                if (pg != null)
                {
                    if (pg.Contains(new DotSpatial.Topology.Point(pt)))
                    {
                        dr = f.DataRow;
                        return Convert.ToSingle(dr["ZONE"]);
                    }
                }
                else
                {
                    // If you have a multi-part polygon then this should also handle holes I think
                    MultiPolygon polygons = f.BasicGeometry as MultiPolygon;
                    if (polygons.Contains(new DotSpatial.Topology.Point(pt)))
                    {
                        dr = f.DataRow;
                        return Convert.ToSingle(dr["ZONE"]);
                    }
                }
            }
            return 0;
        }

        private DateTime FromDateTimeOffset(long aTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(aTime);
        }

        public int GetSites()
        {
            fMain.appManager.UpdateProgress("Downloading weather stations ....");
            selExtent = fMain.appManager.Map.Extent;

            if (GetAreaOfInterest())
            {
                switch (lDatasource)
                {
                    case (int)DataSource.NLDAS:
                        nStation = GetGridForAOI_NLDAS();
                        if (nStation > 0)
                            DrawGridLayer("NLDAS");
                        break;
                    case (int)DataSource.GLDAS:
                        nStation = GetGridForAOI_GLDAS();
                        if (nStation > 0)
                            DrawGridLayer("GLDAS");
                        break;
                    case (int)DataSource.TRMM:
                        nStation = GetGridForAOI_TRMM();
                        if (nStation > 0)
                            DrawGridLayer("TRMM");
                        break;
                    case (int)DataSource.CMIP6:
                        nStation = GetGridForAOI_CMIP6();
                        if (nStation > 0)
                            DrawGridLayer("CMIP6");
                        break;
                    case (int)DataSource.EDDE:
                        nStation = GetGridForAOI_EDDE();
                        if (nStation > 0)
                            DrawGridLayer("EDDE");
                        break;
                    case (int)DataSource.PRISM:
                        nStation = GetGridForAOI_PRISM();
                        if (nStation > 0)
                            DrawGridLayer("PRISM");
                        break;
                    default:
                        nStation = GetSitesForAOI();
                        if (nStation > 0)
                            DrawGagesLayer();
                        break;
                }
                DisableLayerSelection();
            }
            fMain.WriteLogFile("Number of Stations = " + nStation.ToString());
            return nStation;
        }
        private void DisableLayerSelection()
        {
            IMap map = fMain.appManager.Map;
            foreach (IMapLayer lay in map.GetLayers())
            {
                if (!lay.LegendText.Contains("Stations"))
                {
                    lay.IsSelected = false;
                    lay.SelectionEnabled = false;
                }
                else
                {
                    lay.IsSelected = true;
                    lay.SelectionEnabled = true;
                }
            }
        }

        private void ShowError(string msg, Exception ex)
        {
            string crlf = Environment.NewLine;
            msg += crlf + crlf + ex.Message + crlf + crlf + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
