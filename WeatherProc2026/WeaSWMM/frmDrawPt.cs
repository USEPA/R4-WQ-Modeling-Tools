using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using DotSpatial.Controls;
using DotSpatial.Topology;
using DotSpatial.Projections;
using DotSpatial.Data;
using DotSpatial.Symbology;

namespace NCEIData
{
    public partial class frmDrawPt : Form
    {
        private Map appMap;

        //selection from map
        private FeatureSet mapPoint;
        private MapPointLayer mapPointLayer;
        public enum SelectMode { Select, DrawPoint, None };
        private int MapMode = (int)SelectMode.None;
        public bool PointSelected = false;
        private double Xlon;
        private double Ylat;
        private int numPoints;
        private List<SWMMPoint> lstOfPoints = new List<SWMMPoint>();
        private frmSWMM fSWMM;

        public frmDrawPt(frmSWMM _fSWMM, Map _map)
        {
            InitializeComponent();
            appMap = _map;
            fSWMM = _fSWMM;
            appMap.MouseClick += new System.Windows.Forms.MouseEventHandler(appMap_MouseClick);
            SelectPointsFromMap();
        }
        private void SelectPointsFromMap()
        {
            // provide instructions
            //string lbl = "Right click on map to select point ...";
            //appManager.UpdateProgress(lbl);
            // clear drawing layer if present
            MapMode = (int)SelectMode.DrawPoint;

            // clear drawing layer if exist
            if (appMap.MapFrame.DrawingLayers.Contains(mapPointLayer))
            {
                // Remove our drawing layer from the map.
                appMap.MapFrame.DrawingLayers.Remove(mapPointLayer);

                // Request a redraw
                appMap.MapFrame.Invalidate();
            }

            //set map for selection of point
            // Enable left click panning and mouse wheel zooming
            appMap.FunctionMode = FunctionMode.Pan;
            appMap.Cursor = Cursors.Cross;

            // The FeatureSet starts with no data; be sure to set it to the point featuretype
            mapPoint = new FeatureSet(FeatureType.Point);

            // The MapPointLayer controls the drawing of the marker features
            mapPointLayer = new MapPointLayer(mapPoint);

            // The Symbolizer controls what the points look like
            mapPointLayer.Symbolizer = new PointSymbolizer(Color.Blue, DotSpatial.Symbology.PointShape.Ellipse, 12);

            // A drawing layer draws on top of data layers, but is still georeferenced.
            appMap.MapFrame.DrawingLayers.Add(mapPointLayer);
        }

        public int NumOfPoints
        {
            get { return numPoints; }
            set { numPoints = value; } 
        }

        public List<SWMMPoint> lstOfSWMMPoints
        {
            get { return lstOfPoints; }
            set { lstOfPoints = value; }
        }
        private void appMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (MapMode == (int)SelectMode.DrawPoint)
            {
                //intercept left button click
                if (e.Button != MouseButtons.Left) return;
                // Get the geographic location that was clicked
                Coordinate c = appMap.PixelToProj(e.Location);

                Debug.WriteLine("selected ex=" + c.X);
                Debug.WriteLine("selected ey=" + c.Y);

                double[] xy = new double[2];
                double[] z = new double[1];
                z[0] = 1;
                xy[0] = c.X;
                xy[1] = c.Y;

                ProjectionInfo pE = KnownCoordinateSystems.Geographic.World.WGS1984;
                ProjectionInfo pS = KnownCoordinateSystems.Projected.World.WebMercator;
                Debug.WriteLine("In mouseclick: WGS1984 lon={0}, lat={1}", xy[0], xy[1]);

                Reproject.ReprojectPoints(xy, z, pS, pE, 0, 1);

                Xlon = xy[0];
                Ylat = xy[1];
                string txtLat= string.Format("{0:0.00000}", Ylat);
                string txtLon = string.Format("{0:0.00000}", Xlon);

                //add point to list
                SWMMPoint pt = new SWMMPoint();
                pt.X = Xlon;
                pt.Y = Ylat;
                lstOfPoints.Add(pt);
                pt = null;

                Debug.WriteLine("In mouseclick reproject: textlon={0}, textlat={1}", txtLon, txtLat);

                //add point to drawing layers
                Debug.WriteLine("num point est features = " + mapPoint.Features.Count);
                mapPoint.AddFeature(new DotSpatial.Topology.Point(c));
                Debug.WriteLine("num point est features = " + mapPoint.Features.Count);
                appMap.MapFrame.Invalidate();
            }
            else
                return;
            fSWMM.lstOfPoints = lstOfPoints;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            numPoints = mapPoint.Features.Count;
            Debug.WriteLine("In btn OK, num point=" + numPoints.ToString());
            // Remove our drawing layer from the map.
            appMap.MapFrame.DrawingLayers.Remove(mapPointLayer);
            // Request a redraw
            appMap.MapFrame.Invalidate();
            appMap.MouseClick -= new System.Windows.Forms.MouseEventHandler(appMap_MouseClick);
            MapMode = (int)SelectMode.None;
            Cursor.Current = Cursors.Default;
            fSWMM.lstOfPoints = lstOfPoints;

            this.Hide();
        }
    }
}
