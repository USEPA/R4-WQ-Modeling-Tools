using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Topology;
using DotSpatial.Projections;
using DotSpatial.Symbology;
using System.ComponentModel.Composition;


namespace FREQANAL
{
    public partial class frmMap : Form
    {
       // [Export("Shell", typeof(ContainerControl))]
        //private static ContainerControl Shell;

        private string dataDir, selfs;
        private DataTable tblSelGages;
        private DataTable tblUSGSGages;
        private IFeatureLayer selLayer;
        private List<IFeature> lstFeature;
        private frmMain mainForm;
        private string mapMode;
        private double xlon, ylat;
        private List<string> selectedGages;

        //selected estimation point
        private FeatureSet estPoint;
        private MapPointLayer estPointLayer;
        
        public frmMap(frmMain fMain, string dir, DataTable tblGages)
        {
            InitializeComponent();
            //if (DesignMode) return;
            //Shell = this;

            try
            {
               // appManager.LoadExtensions();
                //appManager.HeaderControl.Remove("kHome");
                //appManager.HeaderControl.Remove("kExtensions");
               // appManager.HeaderControl.Remove("None");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }

            this.mainForm = fMain;
            this.dataDir = mainForm.dataDir.ToString();
            this.tblUSGSGages = tblGages;

            CreateMap();

            foreach (Control c in this.Controls)
            {
                if (c is ComboBox)
                {
                    string s = ((ComboBox)c).Text;
                    Debug.WriteLine(" combo name = " + s);
                }
            }
        }

        private void CreateMap() 
        {
            appManager.Map.ClearLayers();
            appManager.Map.Projection = KnownCoordinateSystems.Projected.World.WebMercator;

            //dataDir = "D:\\WINAPPS\\USGS\\GIS";
            string shp1 = dataDir + "\\" + "HUC8WebMerc.shp";
            //string shp2 = dataDir + "\\" + "R4USGSWEB.shp";
            string shp2 = dataDir + "\\" + "USGSWEBMerc.shp";
            //string shp2 = dataDir + "\\" + "remainingsites.shp";
            selfs = dataDir + "\\" + "SelectedGages.shp";

            try
            {
                //gages layer
                FeatureSet fs2 = (FeatureSet)FeatureSet.Open(shp2);
                //fs2.Reproject(map.Projection);
                fs2.FillAttributes();
                tblUSGSGages = fs2.DataTable;
                MapPointLayer gage = (MapPointLayer)appManager.Map.Layers.Add(fs2);
                PointSymbolizer gagesym = new PointSymbolizer(Color.Green, DotSpatial.Symbology.PointShape.Ellipse, 8);
                gage.SelectionSymbolizer = new PointSymbolizer(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 12);
                gage.Symbolizer = gagesym;
                gage.Symbolizer.SetOutline(Color.Black, 1);
                gage.SelectionSymbolizer.SetOutline(Color.Black, 1);
                gage.LegendText = "USGS Gages";
                gage.DataSet.Name = gage.LegendText;

                int idx = appManager.Map.Layers.IndexOf(gage);
                appManager.Map.Layers.SelectLayer(idx);

                mainForm.tblUSGSGages = this.tblUSGSGages;
                appManager.Map.Refresh();
            }
            catch (Exception ex)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Cannot find GIS base layers ...\r\n");
                msg.Append("Feature dataset USGSWebMerc\r\n");
                msg.Append("should be in the \\data folder of the toolkit.\r\n\r\n");
                msg.Append(ex.Message);

                MessageBox.Show(msg.ToString());
                Application.Exit();
            }
        }
        
        public void AddSelectedGagesLayer()
        {
            selfs = dataDir + "\\" + "SelectedGages.shp";

            try
            {
                if (mapMode == "Estimate")
                {
                    FeatureSet fs3 = (FeatureSet)FeatureSet.Open(selfs);
                    MapPointLayer selectedgages = (MapPointLayer)appManager.Map.Layers.Add(fs3);
                    PointSymbolizer selgagesym = new PointSymbolizer(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 10);
                    selectedgages.SelectionSymbolizer = new PointSymbolizer(Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 12);
                    selectedgages.Symbolizer = selgagesym;
                    selectedgages.Symbolizer.SetOutline(Color.Black, 1);
                    selectedgages.SelectionSymbolizer.SetOutline(Color.Black, 1);
                    selectedgages.LegendText = "Selected Gages";
                }

                appManager.Map.Refresh();
            }
            catch (Exception ex)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Cannot find selected gages layer ...\r\n");
                msg.Append(ex.Message);

                MessageBox.Show(msg.ToString());
                return;
            }
        }

        public void ZoomToSelectedGages()
        {
            selfs = dataDir + "\\" + "SelectedGages.shp";

            try
            {
                if (mapMode == "Estimate")
                {
                    FeatureSet fs3 = (FeatureSet)FeatureSet.Open(selfs);
                    fs3.Reproject(appManager.Map.Projection);
                    Extent ext = fs3.Extent;

                    appManager.Map.ViewExtents = ext;
                    appManager.Map.Refresh();
                }
            }
            catch (Exception ex)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Cannot find selected gages layer ...\r\n");
                msg.Append(ex.Message);

                MessageBox.Show(msg.ToString());
                return;
            }
        }

        public string MapMode
        {
            set { mapMode = value; }
            get { return mapMode; }
        }

        public double Xlon
        {
            set { xlon = value; }
            get { return xlon; }
        }
        
        public double Ylat
        {
            set { ylat = value; }
            get { return ylat; }
        }

        private void map_SelectionChanged(object sender, EventArgs e)
        {
            if (mapMode == "Select")
            {
                lstFeature = new List<IFeature>();
                selLayer = (IFeatureLayer)appManager.Map.Layers.SelectedLayer;
                //Debug.WriteLine("selected layer =" + selLayer);
                lstFeature = selLayer.Selection.ToFeatureList();
                //Debug.WriteLine("selected =" + lstFeature.Count);
                if (lstFeature.Count > 0)
                {
                    btnSelect.Enabled = true;
                }
                else
                {
                    btnSelect.Enabled = false;
                }
                lstFeature = null;
            }
            //else 
            //{
            //    btnSelect.Visible = false;
            //}
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (mapMode == "Select")
            {
                //this.mainForm.selGages.Clear();
            }
            else
            {
                if (map != null && map.MapFrame.DrawingLayers.Contains(estPointLayer))
                {
                    // Remove our drawing layer from the map.
                    map.MapFrame.DrawingLayers.Remove(estPointLayer);

                    // Request a redraw
                    map.MapFrame.Invalidate();
                }
            }
            this.Close();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (mapMode == "Select")
            {
                int ret = GetSelectedGages();
                this.Close();
            }
        }

        public int GetSelectedGages()
        {
            selLayer = (IFeatureLayer)appManager.Map.Layers.SelectedLayer;
            List<IFeature> dts = selLayer.Selection.ToFeatureList();
            IFeatureSet fs = selLayer.Selection.ToFeatureSet();
            fs.FillAttributes();
            tblSelGages = fs.DataTable;
            // save selected featureset
            fs.SaveAs(selfs,true);

            this.mainForm.selGages.Clear();
            string site;
            foreach (DataRow rw in tblSelGages.Rows)
            {
                    site = rw["Site_No"].ToString();
                    this.mainForm.selGages.Add(site);
            }
            return (tblSelGages.Rows.Count);
        }



        public void SetMapControls()
        {
            if (mapMode == "Estimate")
            {
                btnSelect.Text = "Left Click on map to select point ...";
                btnSelect.Visible = true;

                //set map for selection of point
                // Enable left click panning and mouse wheel zooming
                map.FunctionMode = FunctionMode.Pan;

                // The FeatureSet starts with no data; be sure to set it to the point featuretype
                estPoint = new FeatureSet(FeatureType.Point);

                // The MapPointLayer controls the drawing of the marker features
                estPointLayer = new MapPointLayer(estPoint);

                // The Symbolizer controls what the points look like
                estPointLayer.Symbolizer = new PointSymbolizer(Color.Blue, DotSpatial.Symbology.PointShape.Ellipse, 12);

                // A drawing layer draws on top of data layers, but is still georeferenced.
                map.MapFrame.DrawingLayers.Add(estPointLayer);
            }
            else
            {
                btnSelect.Text = "Select Gages";
                btnSelect.Visible = true;
            }
        }

        public void HighLightSelectedStations(List<string> selGages)
        {
            List<ILayer> mLayers = new List<ILayer>();
            mLayers = appManager.Map.GetLayers();

            foreach (var mp in mLayers)
            {
                if (mp.DataSet.Name == "USGS Gages")
                {
                    mp.SelectionEnabled = true;
                    mp.IsSelected = true;
                    selLayer = (IFeatureLayer)mp;
                    //Debug.WriteLine("In HighLightSelectedStations: selLayer "+ selLayer.DataSet.Name);

                    List<string> stgage = new List<string>();

                    foreach (string sgage in selGages)
                    {
                        //Debug.WriteLine("selected gage = " + sgage);
                        string s = "'" + sgage + "'";
                        stgage.Add(s);
                    }
                    string st = string.Join(",", stgage);
                    //Debug.WriteLine("selected = " + st);
                    selLayer.SelectByAttribute("[SITE_NO] IN ("+st+")");
                    break;
                }
            }
            mLayers = null;

        }

        /// <summary>
        /// Select point using left mouse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void map_MouseUp(object sender, MouseEventArgs e)
        {
            // Intercept only the right click for adding markers
            if (e.Button != MouseButtons.Right) return;

            // Get the geographic location that was clicked
            Coordinate c = map.PixelToProj(e.Location);

            //Debug.WriteLine("selected ex=" + c.X);
            //Debug.WriteLine("selected ey=" + c.Y);

            double[] xy = new double[2];
            double[] z = new double[1];
            z[0] = 1;
            xy[0] = c.X;
            xy[1] = c.Y;

            ProjectionInfo pE = KnownCoordinateSystems.Geographic.World.WGS1984;
            //ProjectionInfo pS = KnownCoordinateSystems.Projected.World.Mercatorworld;
            ProjectionInfo pS = KnownCoordinateSystems.Projected.World.WebMercator;
            Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);
            
            Debug.WriteLine("In GetSelected Sites: WGS1984 lon={0}, lat={1}", xy[0], xy[1]);
            Xlon = xy[0];
            Ylat = xy[1];
            //mainForm.txtLat.Text = Ylat.ToString();
            //mainForm.txtLon.Text = Xlon.ToString();
            mainForm.txtLat.Text = string.Format("{0:0.00000}", Ylat);
            mainForm.txtLon.Text = string.Format("{0:0.00000}", Xlon);

            // Add the new coordinate as a "point" to the point featureset
            if (estPoint.Features.Count > 0)
            {
                estPoint.Features.Clear();
                estPoint.AddFeature(new DotSpatial.Topology.Point(c));
            }
            else
            {
                estPoint.AddFeature(new DotSpatial.Topology.Point(c));
            }

            // Drawing will take place from a bitmap buffer, so if data is updated,
            // we need to tell the map to refresh the buffer 
            map.MapFrame.Invalidate();
        }

        private void map_MouseClick(object sender, MouseEventArgs e)
        {
        //    if (mapMode == "Estimate")
        //    {
        //        if (map.FunctionMode == FunctionMode.Select)
        //        {
                    //Debug.WriteLine("x="+e.Location.X);
                    //Debug.WriteLine("y=" + e.Location.Y);
        //            Coordinate cx = map.PixelToProj(e.Location);
        //            Debug.WriteLine("selected ex=" + cx.X);
        //            Debug.WriteLine("selected ey=" + cx.Y);

        //            double[] xy = new double[2];
        //            double[] z = new double[1];
        //            z[0] = 1;
        //            xy[0] = cx.X;
        //            xy[1] = cx.Y;

        //            ProjectionInfo pE = KnownCoordinateSystems.Geographic.World.WGS1984;
                    //ProjectionInfo pS = KnownCoordinateSystems.Projected.World.Mercatorworld;
        //            ProjectionInfo pS = KnownCoordinateSystems.Projected.World.WebMercator;
        //            Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);
                    //Debug.WriteLine("In GetSelected Sites: WGS1984 lon={0}, lat={1}", xy[0], xy[1]);
        //            Xlon = xy[0];
        //            Ylat = xy[1];
        //            mainForm.txtLat.Text = Ylat.ToString();
        //            mainForm.txtLon.Text = Xlon.ToString();
        //        }
        //    }
        //    else if (mapMode == "Select")
        //    {
        //        return;
        //    }
        }
    }
}
